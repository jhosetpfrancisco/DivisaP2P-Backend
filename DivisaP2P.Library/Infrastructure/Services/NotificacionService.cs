using DivisaP2P.Library.Core.Common;
using DivisaP2P.Library.Core.DTOs;
using DivisaP2P.Library.Core.Entities;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.Library.Infrastructure.Services;

/// <summary>
/// Bandeja de notificaciones in-app (US-021). Conserva el historial de los
/// últimos 30 días y la usan los demás servicios para avisar de eventos.
/// </summary>
public class NotificacionService : INotificacionService
{
    private const int DiasHistorial = 30;

    private readonly INotificacionRepository _notificaciones;

    public NotificacionService(INotificacionRepository notificaciones)
    {
        _notificaciones = notificaciones;
    }

    public async Task<IEnumerable<NotificacionDto>> ListarAsync(int usuarioId)
    {
        var items = await _notificaciones.GetByUsuarioAsync(usuarioId, DiasHistorial);
        return items.Select(Mapeos.ANotificacionDto);
    }

    public Task<int> ContarNoLeidasAsync(int usuarioId) => _notificaciones.ContarNoLeidasAsync(usuarioId);

    public async Task<ResultadoServicio> MarcarLeidaAsync(int usuarioId, int notificacionId)
    {
        var notificacion = await _notificaciones.GetByIdAsync(notificacionId);
        if (notificacion is null || notificacion.UsuarioId != usuarioId)
            return ResultadoServicio.Error(CodigoError.NoEncontrado, "Notificación no encontrada.");

        if (!notificacion.Leida)
        {
            notificacion.Leida = true;
            await _notificaciones.UpdateAsync(notificacion);
        }
        return ResultadoServicio.Ok();
    }

    public Task MarcarTodasLeidasAsync(int usuarioId) => _notificaciones.MarcarTodasLeidasAsync(usuarioId);

    public async Task CrearAsync(int usuarioId, string titulo, string descripcion, string? enlace = null)
    {
        await _notificaciones.AddAsync(new Notificacion
        {
            UsuarioId = usuarioId,
            Titulo = titulo,
            Descripcion = descripcion,
            Enlace = enlace,
            Leida = false,
            Fecha = DateTime.UtcNow
        });
    }
}
