using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.WebApi.Controllers;

/// <summary>Bandeja de notificaciones del usuario autenticado (US-021).</summary>
[Authorize]
public class NotificacionesController : ApiControllerBase
{
    private readonly INotificacionService _notificaciones;

    public NotificacionesController(INotificacionService notificaciones)
    {
        _notificaciones = notificaciones;
    }

    // GET: api/notificaciones
    [HttpGet]
    public async Task<IActionResult> Listar() =>
        Ok(await _notificaciones.ListarAsync(UsuarioId));

    // GET: api/notificaciones/no-leidas  (contador para la campana)
    [HttpGet("no-leidas")]
    public async Task<IActionResult> ContarNoLeidas() =>
        Ok(new { cantidad = await _notificaciones.ContarNoLeidasAsync(UsuarioId) });

    // PUT: api/notificaciones/5/leer
    [HttpPut("{id:int}/leer")]
    public async Task<IActionResult> MarcarLeida(int id) =>
        Responder(await _notificaciones.MarcarLeidaAsync(UsuarioId, id));

    // PUT: api/notificaciones/leer-todas
    [HttpPut("leer-todas")]
    public async Task<IActionResult> MarcarTodasLeidas()
    {
        await _notificaciones.MarcarTodasLeidasAsync(UsuarioId);
        return Ok(new { mensaje = "Todas las notificaciones fueron marcadas como leídas." });
    }
}
