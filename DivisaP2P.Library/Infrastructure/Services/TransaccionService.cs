using DivisaP2P.Library.Core.Common;
using DivisaP2P.Library.Core.DTOs;
using DivisaP2P.Library.Core.Entities;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.Library.Infrastructure.Services;

/// <summary>
/// Ciclo de vida de una transacción (US-008 a US-011).
///
/// Supuesto de roles (regla de negocio del equipo): el usuario que TOMA la oferta
/// es el Comprador (paga primero en la divisa origen); el usuario que PUBLICÓ la
/// oferta es el Vendedor (entrega después en la divisa destino).
///
/// Máquina de estados:
///   PendientePago --(comprador reporta pago)--> PagoReportado
///   PagoReportado --(vendedor valida: aprueba)--> PagoConfirmado
///   PagoReportado --(vendedor valida: rechaza)--> EnDisputa
///   PagoConfirmado --(vendedor reporta entrega)--> EntregaReportada
///   EntregaReportada --(comprador valida: aprueba)--> Completada
///   EntregaReportada --(comprador valida: rechaza)--> EnDisputa
/// </summary>
public class TransaccionService : ITransaccionService
{
    private readonly ITransaccionRepository _transacciones;
    private readonly IOfertaRepository _ofertas;
    private readonly IUsuarioRepository _usuarios;
    private readonly INotificacionService _notificaciones;

    public TransaccionService(
        ITransaccionRepository transacciones,
        IOfertaRepository ofertas,
        IUsuarioRepository usuarios,
        INotificacionService notificaciones)
    {
        _transacciones = transacciones;
        _ofertas = ofertas;
        _usuarios = usuarios;
        _notificaciones = notificaciones;
    }

    public async Task<ResultadoServicio<TransaccionDto>> IniciarAsync(int usuarioId, IniciarTransaccionDto dto)
    {
        var oferta = await _ofertas.GetByIdAsync(dto.OfertaId);
        if (oferta is null)
            return ResultadoServicio<TransaccionDto>.Error(CodigoError.NoEncontrado, "Oferta no encontrada.");

        if (oferta.Estado != EstadosOferta.Activa)
            return ResultadoServicio<TransaccionDto>.Error(CodigoError.Validacion, "La oferta ya no está activa.");

        if (oferta.FechaExpiracion <= DateTime.UtcNow)
            return ResultadoServicio<TransaccionDto>.Error(CodigoError.Validacion, "La oferta ha expirado.");

        if (oferta.UsuarioId == usuarioId)
            return ResultadoServicio<TransaccionDto>.Error(CodigoError.Validacion,
                "No puedes iniciar una transacción sobre tu propia oferta.");

        if (dto.Monto < ReglasNegocio.MontoMinimoOferta)
            return ResultadoServicio<TransaccionDto>.Error(CodigoError.Validacion,
                $"El monto mínimo a operar es {ReglasNegocio.MontoMinimoOferta}.");

        if (dto.Monto > oferta.MontoDisponible)
            return ResultadoServicio<TransaccionDto>.Error(CodigoError.Validacion,
                $"El monto excede el disponible de la oferta ({oferta.MontoDisponible}).");

        var ahora = DateTime.UtcNow;
        var transaccion = new Transaccion
        {
            Codigo = await GenerarCodigoUnicoAsync(),
            OfertaId = oferta.Id,
            CompradorId = usuarioId,
            VendedorId = oferta.UsuarioId,
            MontoOperado = dto.Monto,
            TipoCambio = oferta.TipoCambio,
            Estado = EstadosTransaccion.PendientePago,
            FechaInicio = ahora,
            FechaLimiteAccion = ahora.AddMinutes(ReglasNegocio.MinutosReportePago),
            FechaActualizacion = ahora
        };
        transaccion.Historial.Add(new HistorialEstadoTransaccion
        {
            Estado = EstadosTransaccion.PendientePago,
            Comentario = "Transacción iniciada.",
            Fecha = ahora
        });

        // Reservar el monto: descontarlo del disponible de la oferta (US-008).
        oferta.MontoDisponible -= dto.Monto;
        if (oferta.MontoDisponible < ReglasNegocio.MontoMinimoOferta)
            oferta.Estado = EstadosOferta.Agotada; // RN-008
        await _ofertas.UpdateAsync(oferta);

        var creada = await _transacciones.AddAsync(transaccion);

        await _notificaciones.CrearAsync(transaccion.CompradorId, "Transacción iniciada",
            $"Iniciaste la transacción {transaccion.Codigo}. Tienes {ReglasNegocio.MinutosReportePago} minutos para reportar el pago.");
        await _notificaciones.CrearAsync(transaccion.VendedorId, "Nueva transacción",
            $"Un usuario tomó tu oferta (transacción {transaccion.Codigo}).");

        var detalle = await _transacciones.GetByIdAsync(creada.Id);
        return ResultadoServicio<TransaccionDto>.Ok(Mapeos.ATransaccionDto(detalle!), "Transacción iniciada.");
    }

    public async Task<ResultadoServicio> ReportarDepositoAsync(int usuarioId, int transaccionId, ReporteDepositoDto dto)
    {
        var t = await _transacciones.GetByIdAsync(transaccionId);
        if (t is null)
            return ResultadoServicio.Error(CodigoError.NoEncontrado, "Transacción no encontrada.");

        string tipoVoucher;
        string nuevoEstado;

        // Caso (a): el comprador reporta el pago en la divisa origen.
        if (t.Estado == EstadosTransaccion.PendientePago && usuarioId == t.CompradorId)
        {
            tipoVoucher = TipoVoucher.Pago;
            nuevoEstado = EstadosTransaccion.PagoReportado;
        }
        // Caso (b): el vendedor reporta la entrega en la divisa destino.
        else if (t.Estado == EstadosTransaccion.PagoConfirmado && usuarioId == t.VendedorId)
        {
            tipoVoucher = TipoVoucher.Entrega;
            nuevoEstado = EstadosTransaccion.EntregaReportada;
        }
        else
        {
            return ResultadoServicio.Error(CodigoError.NoAutorizado,
                "No puedes reportar un depósito en el estado actual de la transacción.");
        }

        var ahora = DateTime.UtcNow;
        t.Vouchers.Add(new Voucher
        {
            UsuarioId = usuarioId,
            Tipo = tipoVoucher,
            RutaArchivo = dto.RutaArchivo,
            NombreArchivo = dto.NombreArchivo,
            NumeroOperacion = dto.NumeroOperacion,
            FechaDeposito = dto.FechaDeposito,
            FechaSubida = ahora
        });
        CambiarEstado(t, nuevoEstado, ahora.AddHours(ReglasNegocio.HorasValidacionPago),
            "Depósito reportado, pendiente de validación.", ahora);

        await _transacciones.UpdateAsync(t);

        var contraparte = tipoVoucher == TipoVoucher.Pago ? t.VendedorId : t.CompradorId;
        await _notificaciones.CrearAsync(contraparte, "Depósito reportado",
            $"La contraparte reportó un depósito en la transacción {t.Codigo}. Valídalo dentro de {ReglasNegocio.HorasValidacionPago} horas.");

        return ResultadoServicio.Ok("Depósito reportado correctamente.");
    }

    public async Task<ResultadoServicio> ValidarDepositoAsync(int usuarioId, int transaccionId, ValidacionDepositoDto dto)
    {
        var t = await _transacciones.GetByIdAsync(transaccionId);
        if (t is null)
            return ResultadoServicio.Error(CodigoError.NoEncontrado, "Transacción no encontrada.");

        if (!dto.Aprobar && (string.IsNullOrWhiteSpace(dto.MotivoRechazo) || dto.MotivoRechazo.Trim().Length < 20))
            return ResultadoServicio.Error(CodigoError.Validacion,
                "El motivo de rechazo es obligatorio y debe tener al menos 20 caracteres.");

        var ahora = DateTime.UtcNow;

        // Caso (a): el vendedor valida el pago del comprador.
        if (t.Estado == EstadosTransaccion.PagoReportado && usuarioId == t.VendedorId)
        {
            if (dto.Aprobar)
            {
                CambiarEstado(t, EstadosTransaccion.PagoConfirmado,
                    ahora.AddMinutes(ReglasNegocio.MinutosReportePago),
                    "Pago confirmado por el vendedor. Pendiente de entrega de la divisa.", ahora);
                await _transacciones.UpdateAsync(t);
                await _notificaciones.CrearAsync(t.CompradorId, "Pago confirmado",
                    $"Tu pago en la transacción {t.Codigo} fue confirmado.");
                return ResultadoServicio.Ok("Pago confirmado.");
            }
            return await PasarADisputaPorRechazoAsync(t, dto.MotivoRechazo!, ahora);
        }

        // Caso (b): el comprador valida la entrega del vendedor.
        if (t.Estado == EstadosTransaccion.EntregaReportada && usuarioId == t.CompradorId)
        {
            if (dto.Aprobar)
            {
                CambiarEstado(t, EstadosTransaccion.Completada, null,
                    "Entrega confirmada por el comprador. Transacción completada.", ahora);
                await _transacciones.UpdateAsync(t);
                await IncrementarOperacionesAsync(t);
                await _notificaciones.CrearAsync(t.VendedorId, "Transacción completada",
                    $"La transacción {t.Codigo} se completó exitosamente.");
                await _notificaciones.CrearAsync(t.CompradorId, "Transacción completada",
                    $"La transacción {t.Codigo} se completó. Ya puedes calificar a la contraparte.");
                return ResultadoServicio.Ok("Transacción completada.");
            }
            return await PasarADisputaPorRechazoAsync(t, dto.MotivoRechazo!, ahora);
        }

        return ResultadoServicio.Error(CodigoError.NoAutorizado,
            "No puedes validar un depósito en el estado actual de la transacción.");
    }

    public async Task<ResultadoServicio<TransaccionDetalleDto>> GetDetalleAsync(int usuarioId, int transaccionId)
    {
        var t = await _transacciones.GetDetalleAsync(transaccionId);
        if (t is null)
            return ResultadoServicio<TransaccionDetalleDto>.Error(CodigoError.NoEncontrado, "Transacción no encontrada.");

        // Solo las partes involucradas pueden ver el detalle.
        if (usuarioId != t.CompradorId && usuarioId != t.VendedorId)
            return ResultadoServicio<TransaccionDetalleDto>.Error(CodigoError.NoAutorizado,
                "No tienes acceso a esta transacción.");

        return ResultadoServicio<TransaccionDetalleDto>.Ok(Mapeos.ATransaccionDetalleDto(t));
    }

    public async Task<ResultadoPaginado<TransaccionDto>> HistorialAsync(
        int usuarioId, DateTime? desde, DateTime? hasta, string? estado, int pagina, int tamanioPagina)
    {
        var (items, total) = await _transacciones.GetHistorialAsync(usuarioId, desde, hasta, estado, pagina, tamanioPagina);
        return new ResultadoPaginado<TransaccionDto>
        {
            Data = items.Select(Mapeos.ATransaccionDto),
            Total = total,
            Pagina = pagina,
            TamanioPagina = tamanioPagina
        };
    }

    // ---------- Helpers privados ----------

    private async Task<ResultadoServicio> PasarADisputaPorRechazoAsync(Transaccion t, string motivo, DateTime ahora)
    {
        CambiarEstado(t, EstadosTransaccion.EnDisputa, null,
            "Depósito rechazado: " + motivo, ahora);
        await _transacciones.UpdateAsync(t);
        await _notificaciones.CrearAsync(t.CompradorId, "Transacción en disputa",
            $"La transacción {t.Codigo} pasó a disputa. Un administrador la revisará.");
        await _notificaciones.CrearAsync(t.VendedorId, "Transacción en disputa",
            $"La transacción {t.Codigo} pasó a disputa. Un administrador la revisará.");
        return ResultadoServicio.Ok("La transacción pasó a disputa.");
    }

    private static void CambiarEstado(Transaccion t, string nuevoEstado, DateTime? limiteAccion, string comentario, DateTime ahora)
    {
        t.Estado = nuevoEstado;
        t.FechaLimiteAccion = limiteAccion;
        t.FechaActualizacion = ahora;
        t.Historial.Add(new HistorialEstadoTransaccion
        {
            Estado = nuevoEstado,
            Comentario = comentario,
            Fecha = ahora
        });
    }

    private async Task IncrementarOperacionesAsync(Transaccion t)
    {
        var comprador = await _usuarios.GetByIdAsync(t.CompradorId);
        var vendedor = await _usuarios.GetByIdAsync(t.VendedorId);
        if (comprador is not null)
        {
            comprador.OperacionesCompletadas++;
            await _usuarios.UpdateAsync(comprador);
        }
        if (vendedor is not null)
        {
            vendedor.OperacionesCompletadas++;
            await _usuarios.UpdateAsync(vendedor);
        }
    }

    private async Task<string> GenerarCodigoUnicoAsync()
    {
        var random = new Random();
        string codigo;
        do
        {
            codigo = $"TXN-{random.Next(10000, 99999)}";
        } while (await _transacciones.CodigoExisteAsync(codigo));
        return codigo;
    }
}
