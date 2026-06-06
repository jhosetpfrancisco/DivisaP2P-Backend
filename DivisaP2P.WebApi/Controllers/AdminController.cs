using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DivisaP2P.Library.Core.Common;
using DivisaP2P.Library.Core.DTOs;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.WebApi.Controllers;

/// <summary>
/// Panel administrativo: dashboard (US-016), gestión de usuarios (US-017),
/// aprobación de empresas (US-019) y reportes exportables (US-018).
/// Todo el controlador requiere rol Administrador.
/// </summary>
[Authorize(Roles = Roles.Administrador)]
[Route("api/admin")]
public class AdminController : ApiControllerBase
{
    private readonly IAdminService _admin;
    private readonly IUsuarioService _usuarios;

    public AdminController(IAdminService admin, IUsuarioService usuarios)
    {
        _admin = admin;
        _usuarios = usuarios;
    }

    // GET: api/admin/dashboard
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard() =>
        Ok(await _admin.GetDashboardAsync());

    // GET: api/admin/usuarios?rol=USU&estado=Activo&calificacionMin=4&pagina=1
    [HttpGet("usuarios")]
    public async Task<IActionResult> Usuarios(
        [FromQuery] string? rol, [FromQuery] string? estado, [FromQuery] decimal? calificacionMin,
        [FromQuery] int pagina = 1, [FromQuery] int tamanioPagina = 10) =>
        Ok(await _usuarios.ListarAsync(rol, estado, calificacionMin, pagina, tamanioPagina));

    // POST: api/admin/usuarios/5/bloquear
    [HttpPost("usuarios/{id:int}/bloquear")]
    public async Task<IActionResult> Bloquear(int id, BloqueoUsuarioDto dto) =>
        Responder(await _usuarios.BloquearAsync(id, dto));

    // POST: api/admin/usuarios/5/desbloquear
    [HttpPost("usuarios/{id:int}/desbloquear")]
    public async Task<IActionResult> Desbloquear(int id) =>
        Responder(await _usuarios.DesbloquearAsync(id));

    // POST: api/admin/empresas/5/aprobar
    [HttpPost("empresas/{id:int}/aprobar")]
    public async Task<IActionResult> AprobarEmpresa(int id) =>
        Responder(await _usuarios.AprobarEmpresaAsync(id));

    // GET: api/admin/reportes/transacciones?desde=2026-01-01&estado=Completada
    [HttpGet("reportes/transacciones")]
    public async Task<IActionResult> ReporteTransacciones(
        [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, [FromQuery] string? estado,
        [FromQuery] int pagina = 1, [FromQuery] int tamanioPagina = 20) =>
        Ok(await _admin.ReporteTransaccionesAsync(desde, hasta, estado, pagina, tamanioPagina));

    // GET: api/admin/reportes/transacciones/export  (descarga CSV para Excel)
    [HttpGet("reportes/transacciones/export")]
    public async Task<IActionResult> ExportarTransacciones(
        [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, [FromQuery] string? estado)
    {
        var csv = await _admin.ExportarTransaccionesCsvAsync(desde, hasta, estado);
        var nombre = $"reporte_transacciones_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";
        return File(csv, "text/csv", nombre);
    }
}
