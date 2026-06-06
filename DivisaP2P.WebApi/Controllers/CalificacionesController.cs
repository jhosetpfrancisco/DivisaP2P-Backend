using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DivisaP2P.Library.Core.DTOs;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.WebApi.Controllers;

/// <summary>Calificación de la contraparte tras una transacción completada (US-012).</summary>
[Authorize]
public class CalificacionesController : ApiControllerBase
{
    private readonly ICalificacionService _calificaciones;

    public CalificacionesController(ICalificacionService calificaciones)
    {
        _calificaciones = calificaciones;
    }

    // POST: api/calificaciones
    [HttpPost]
    public async Task<IActionResult> Calificar(CalificacionCreateDto dto) =>
        Responder(await _calificaciones.CalificarAsync(UsuarioId, dto));
}
