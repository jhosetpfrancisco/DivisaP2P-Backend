using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DivisaP2P.Library.Core.Common;
using DivisaP2P.Library.Core.DTOs;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.WebApi.Controllers;

/// <summary>Apertura de disputas (US-014) y resolución por el administrador (US-015).</summary>
[Authorize]
public class DisputasController : ApiControllerBase
{
    private readonly IDisputaService _disputas;

    public DisputasController(IDisputaService disputas)
    {
        _disputas = disputas;
    }

    // POST: api/disputas  (abrir disputa — cualquier parte de la transacción)
    [HttpPost]
    public async Task<IActionResult> Abrir(DisputaCreateDto dto) =>
        Responder(await _disputas.AbrirAsync(UsuarioId, dto));

    // GET: api/disputas?estado=Abierta&pagina=1  (solo administrador)
    [HttpGet]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? estado, [FromQuery] int pagina = 1, [FromQuery] int tamanioPagina = 10) =>
        Ok(await _disputas.ListarAsync(estado, pagina, tamanioPagina));

    // GET: api/disputas/5  (solo administrador)
    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> GetById(int id)
    {
        var disputa = await _disputas.GetByIdAsync(id);
        return disputa is null ? NotFound() : Ok(disputa);
    }

    // POST: api/disputas/5/resolver  (solo administrador)
    [HttpPost("{id:int}/resolver")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Resolver(int id, ResolucionDisputaDto dto) =>
        Responder(await _disputas.ResolverAsync(UsuarioId, id, dto));
}
