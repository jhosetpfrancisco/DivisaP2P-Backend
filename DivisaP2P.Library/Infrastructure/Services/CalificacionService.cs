using DivisaP2P.Library.Core.Common;
using DivisaP2P.Library.Core.DTOs;
using DivisaP2P.Library.Core.Entities;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.Library.Infrastructure.Services;

/// <summary>
/// Calificación de la contraparte tras completar una transacción (US-012).
/// Recalcula el promedio del usuario calificado al registrar la calificación.
/// </summary>
public class CalificacionService : ICalificacionService
{
    private readonly ICalificacionRepository _calificaciones;
    private readonly ITransaccionRepository _transacciones;
    private readonly IUsuarioRepository _usuarios;

    public CalificacionService(
        ICalificacionRepository calificaciones,
        ITransaccionRepository transacciones,
        IUsuarioRepository usuarios)
    {
        _calificaciones = calificaciones;
        _transacciones = transacciones;
        _usuarios = usuarios;
    }

    public async Task<ResultadoServicio<CalificacionDto>> CalificarAsync(int usuarioId, CalificacionCreateDto dto)
    {
        var t = await _transacciones.GetByIdAsync(dto.TransaccionId);
        if (t is null)
            return ResultadoServicio<CalificacionDto>.Error(CodigoError.NoEncontrado, "Transacción no encontrada.");

        if (usuarioId != t.CompradorId && usuarioId != t.VendedorId)
            return ResultadoServicio<CalificacionDto>.Error(CodigoError.NoAutorizado,
                "No participaste en esta transacción.");

        if (t.Estado != EstadosTransaccion.Completada)
            return ResultadoServicio<CalificacionDto>.Error(CodigoError.Validacion,
                "Solo puedes calificar transacciones completadas.");

        // Plazo de 7 días desde la completitud (RN-015).
        if (DateTime.UtcNow > t.FechaActualizacion.AddDays(ReglasNegocio.DiasParaCalificar))
            return ResultadoServicio<CalificacionDto>.Error(CodigoError.Validacion,
                "El plazo de 7 días para calificar ya venció.");

        if (await _calificaciones.YaCalificoAsync(dto.TransaccionId, usuarioId))
            return ResultadoServicio<CalificacionDto>.Error(CodigoError.Conflicto,
                "Ya calificaste esta transacción.");

        var calificadoId = usuarioId == t.CompradorId ? t.VendedorId : t.CompradorId;

        var calificacion = new Calificacion
        {
            TransaccionId = dto.TransaccionId,
            CalificadorId = usuarioId,
            CalificadoId = calificadoId,
            Estrellas = dto.Estrellas,
            Comentario = dto.Comentario,
            Fecha = DateTime.UtcNow
        };
        var creada = await _calificaciones.AddAsync(calificacion);

        // Actualizar el promedio del usuario calificado (US-012).
        var (promedio, _) = await _calificaciones.GetPromedioAsync(calificadoId);
        var calificado = await _usuarios.GetByIdAsync(calificadoId);
        if (calificado is not null)
        {
            calificado.CalificacionPromedio = decimal.Round(promedio, 2);
            await _usuarios.UpdateAsync(calificado);
        }

        return ResultadoServicio<CalificacionDto>.Ok(Mapeos.ACalificacionDto(creada), "Calificación registrada.");
    }
}
