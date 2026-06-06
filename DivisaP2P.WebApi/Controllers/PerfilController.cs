using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DivisaP2P.Library.Core.DTOs;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.WebApi.Controllers;

/// <summary>Perfil del usuario autenticado y sus cuentas bancarias (US-003).</summary>
[Authorize]
public class PerfilController : ApiControllerBase
{
    private readonly IUsuarioService _usuarios;
    private readonly ICuentaBancariaService _cuentas;

    public PerfilController(IUsuarioService usuarios, ICuentaBancariaService cuentas)
    {
        _usuarios = usuarios;
        _cuentas = cuentas;
    }

    // GET: api/perfil
    [HttpGet]
    public async Task<IActionResult> GetPerfil()
    {
        var perfil = await _usuarios.GetPerfilAsync(UsuarioId);
        return perfil is null ? NotFound() : Ok(perfil);
    }

    // PUT: api/perfil
    [HttpPut]
    public async Task<IActionResult> Actualizar(PerfilUpdateDto dto) =>
        Responder(await _usuarios.ActualizarPerfilAsync(UsuarioId, dto));

    // PUT: api/perfil/password
    [HttpPut("password")]
    public async Task<IActionResult> CambiarPassword(CambioPasswordDto dto) =>
        Responder(await _usuarios.CambiarPasswordAsync(UsuarioId, dto));

    // GET: api/perfil/cuentas
    [HttpGet("cuentas")]
    public async Task<IActionResult> ListarCuentas() =>
        Ok(await _cuentas.ListarAsync(UsuarioId));

    // POST: api/perfil/cuentas
    [HttpPost("cuentas")]
    public async Task<IActionResult> CrearCuenta(CuentaBancariaCreateDto dto) =>
        Responder(await _cuentas.CrearAsync(UsuarioId, dto));

    // DELETE: api/perfil/cuentas/5
    [HttpDelete("cuentas/{id:int}")]
    public async Task<IActionResult> EliminarCuenta(int id) =>
        Responder(await _cuentas.EliminarAsync(UsuarioId, id));
}
