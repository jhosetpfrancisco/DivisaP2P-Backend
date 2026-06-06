using DivisaP2P.Library.Core.Common;
using DivisaP2P.Library.Core.DTOs;
using DivisaP2P.Library.Core.Entities;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.Library.Infrastructure.Services;

/// <summary>
/// Apertura de disputas por el usuario (US-014) y resolución por el administrador (US-015).
/// Abrir una disputa congela la transacción; la resolución la cierra con un veredicto.
/// </summary>
public class DisputaService : IDisputaService
{
    private readonly IDisputaRepository _disputas;
    private readonly ITransaccionRepository _transacciones;
    private readonly INotificacionService _notificaciones;

    private static readonly string[] EstadosQuePermitenDisputa =
    {
        EstadosTransaccion.PagoReportado,
        EstadosTransaccion.PagoConfirmado,
        EstadosTransaccion.EntregaReportada
    };

    public DisputaService(
        IDisputaRepository disputas,
        ITransaccionRepository transacciones,
        INotificacionService notificaciones)
    {
        _disputas = disputas;
        _transacciones = transacciones;
        _notificaciones = notificaciones;
    }

    public async Task<ResultadoServicio<DisputaDto>> AbrirAsync(int usuarioId, DisputaCreateDto dto)
    {
        var t = await _transacciones.GetByIdAsync(dto.TransaccionId);
        if (t is null)
            return ResultadoServicio<DisputaDto>.Error(CodigoError.NoEncontrado, "Transacción no encontrada.");

        if (usuarioId != t.CompradorId && usuarioId != t.VendedorId)
            return ResultadoServicio<DisputaDto>.Error(CodigoError.NoAutorizado, "No participaste en esta transacción.");

        if (!EstadosQuePermitenDisputa.Contains(t.Estado))
            return ResultadoServicio<DisputaDto>.Error(CodigoError.Validacion,
                "Solo se puede abrir disputa en estados Pago reportado, Pago confirmado o Entrega reportada.");

        if (await _disputas.GetByTransaccionAsync(dto.TransaccionId) is not null)
            return ResultadoServicio<DisputaDto>.Error(CodigoError.Conflicto, "Esta transacción ya tiene una disputa.");

        var ahora = DateTime.UtcNow;
        var disputa = new Disputa
        {
            TransaccionId = dto.TransaccionId,
            AbiertaPorId = usuarioId,
            Motivo = dto.Motivo,
            Estado = EstadosDisputa.Abierta,
            FechaApertura = ahora,
            Evidencias = dto.Evidencias
                .Take(3)
                .Select(ruta => new DisputaEvidencia
                {
                    RutaArchivo = ruta,
                    NombreArchivo = System.IO.Path.GetFileName(ruta)
                })
                .ToList()
        };
        var creada = await _disputas.AddAsync(disputa);

        // Congelar la transacción (RN-016).
        t.Estado = EstadosTransaccion.EnDisputa;
        t.FechaLimiteAccion = null;
        t.FechaActualizacion = ahora;
        t.Historial.Add(new HistorialEstadoTransaccion
        {
            Estado = EstadosTransaccion.EnDisputa,
            Comentario = "Disputa abierta: " + dto.Motivo,
            Fecha = ahora
        });
        await _transacciones.UpdateAsync(t);

        var contraparte = usuarioId == t.CompradorId ? t.VendedorId : t.CompradorId;
        await _notificaciones.CrearAsync(contraparte, "Disputa abierta",
            $"Se abrió una disputa en la transacción {t.Codigo}. Un administrador la revisará.");

        var conDatos = await _disputas.GetByIdAsync(creada.Id);
        return ResultadoServicio<DisputaDto>.Ok(Mapeos.ADisputaDto(conDatos!), "Disputa abierta.");
    }

    public async Task<ResultadoPaginado<DisputaDto>> ListarAsync(string? estado, int pagina, int tamanioPagina)
    {
        var (items, total) = await _disputas.ListarAsync(estado, pagina, tamanioPagina);
        return new ResultadoPaginado<DisputaDto>
        {
            Data = items.Select(Mapeos.ADisputaDto),
            Total = total,
            Pagina = pagina,
            TamanioPagina = tamanioPagina
        };
    }

    public async Task<DisputaDto?> GetByIdAsync(int id)
    {
        var disputa = await _disputas.GetByIdAsync(id);
        return disputa is null ? null : Mapeos.ADisputaDto(disputa);
    }

    public async Task<ResultadoServicio> ResolverAsync(int adminId, int disputaId, ResolucionDisputaDto dto)
    {
        var disputa = await _disputas.GetByIdAsync(disputaId);
        if (disputa is null)
            return ResultadoServicio.Error(CodigoError.NoEncontrado, "Disputa no encontrada.");

        if (disputa.Estado == EstadosDisputa.Resuelta)
            return ResultadoServicio.Error(CodigoError.Validacion, "La disputa ya fue resuelta.");

        var ahora = DateTime.UtcNow;
        disputa.Estado = EstadosDisputa.Resuelta;
        disputa.Resolucion = dto.Resolucion;
        disputa.ComentarioResolucion = dto.Comentario;
        disputa.FechaResolucion = ahora;
        await _disputas.UpdateAsync(disputa);

        // La transacción cambia de estado según la resolución.
        var t = await _transacciones.GetByIdAsync(disputa.TransaccionId);
        if (t is not null)
        {
            // A favor del comprador o anulada -> se cancela; a favor del vendedor -> se completa.
            t.Estado = dto.Resolucion == ResolucionDisputa.AFavorVendedor
                ? EstadosTransaccion.Completada
                : EstadosTransaccion.Cancelada;
            t.FechaActualizacion = ahora;
            t.Historial.Add(new HistorialEstadoTransaccion
            {
                Estado = t.Estado,
                Comentario = $"Disputa resuelta ({dto.Resolucion}): {dto.Comentario}",
                Fecha = ahora
            });
            await _transacciones.UpdateAsync(t);

            await _notificaciones.CrearAsync(t.CompradorId, "Disputa resuelta",
                $"La disputa de la transacción {t.Codigo} fue resuelta: {dto.Resolucion}.");
            await _notificaciones.CrearAsync(t.VendedorId, "Disputa resuelta",
                $"La disputa de la transacción {t.Codigo} fue resuelta: {dto.Resolucion}.");
        }

        return ResultadoServicio.Ok("Disputa resuelta.");
    }
}
