using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using DivisaP2P.Library.Core.Common;

namespace DivisaP2P.WebApi.Controllers;

/// <summary>
/// Base de los controladores: expone el id/rol del usuario autenticado (desde el
/// token JWT) y traduce un <see cref="ResultadoServicio"/> a la respuesta HTTP.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Id del usuario autenticado, tomado del claim NameIdentifier del JWT.</summary>
    protected int UsuarioId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    protected string Rol => User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    /// <summary>Traduce el resultado de un servicio (sin datos) a IActionResult.</summary>
    protected IActionResult Responder(ResultadoServicio resultado)
    {
        if (resultado.Exito)
            return Ok(new { mensaje = resultado.Mensaje });

        return MapearError(resultado);
    }

    /// <summary>Traduce el resultado de un servicio (con datos) a IActionResult.</summary>
    protected IActionResult Responder<T>(ResultadoServicio<T> resultado)
    {
        if (resultado.Exito)
            return Ok(resultado.Datos);

        return MapearError(resultado);
    }

    private IActionResult MapearError(ResultadoServicio resultado)
    {
        var cuerpo = new { mensaje = resultado.Mensaje };
        return resultado.Codigo switch
        {
            CodigoError.NoEncontrado => NotFound(cuerpo),
            CodigoError.NoAutorizado => StatusCode(StatusCodes.Status403Forbidden, cuerpo),
            CodigoError.Conflicto => Conflict(cuerpo),
            _ => BadRequest(cuerpo)
        };
    }
}
