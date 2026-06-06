using Microsoft.AspNetCore.Mvc;
using DivisaP2P.Library.Core.DTOs;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.WebApi.Controllers;

/// <summary>Registro e inicio de sesión (US-001, US-002, US-019). Endpoints públicos.</summary>
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    // POST: api/auth/registro
    [HttpPost("registro")]
    public async Task<IActionResult> Registrar(RegistroUsuarioDto dto) =>
        Responder(await _auth.RegistrarUsuarioAsync(dto));

    // POST: api/auth/registro-empresa
    [HttpPost("registro-empresa")]
    public async Task<IActionResult> RegistrarEmpresa(RegistroEmpresaDto dto) =>
        Responder(await _auth.RegistrarEmpresaAsync(dto));

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto) =>
        Responder(await _auth.LoginAsync(dto));

    // POST: api/auth/verificar/5  (simula el clic en el enlace del correo)
    [HttpPost("verificar/{usuarioId:int}")]
    public async Task<IActionResult> Verificar(int usuarioId) =>
        Responder(await _auth.VerificarCorreoAsync(usuarioId));
}
