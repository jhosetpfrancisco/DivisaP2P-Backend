using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DivisaP2P.Library.Core.DTOs;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.WebApi.Controllers;

/// <summary>Publicación, búsqueda, edición y matching de ofertas (US-004 a US-007).</summary>
[Authorize]
public class OfertasController : ApiControllerBase
{
    private readonly IOfertaService _ofertas;

    public OfertasController(IOfertaService ofertas)
    {
        _ofertas = ofertas;
    }

    // GET: api/ofertas?tipoOperacion=Compra&divisa=USD&ordenarPor=tipoCambio&pagina=1
    [HttpGet]
    public async Task<IActionResult> Buscar([FromQuery] OfertaFiltroDto filtro) =>
        Ok(await _ofertas.BuscarAsync(filtro, UsuarioId));

    // GET: api/ofertas/mias
    [HttpGet("mias")]
    public async Task<IActionResult> MisOfertas() =>
        Ok(await _ofertas.MisOfertasAsync(UsuarioId));

    // GET: api/ofertas/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var oferta = await _ofertas.GetByIdAsync(id);
        return oferta is null ? NotFound() : Ok(oferta);
    }

    // GET: api/ofertas/5/matches
    [HttpGet("{id:int}/matches")]
    public async Task<IActionResult> Matches(int id) =>
        Responder(await _ofertas.MatchingAsync(UsuarioId, id));

    // POST: api/ofertas
    [HttpPost]
    public async Task<IActionResult> Publicar(OfertaCreateDto dto) =>
        Responder(await _ofertas.PublicarAsync(UsuarioId, Rol, dto));

    // PUT: api/ofertas/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, OfertaUpdateDto dto) =>
        Responder(await _ofertas.ActualizarTipoCambioAsync(UsuarioId, id, dto));

    // DELETE: api/ofertas/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancelar(int id) =>
        Responder(await _ofertas.CancelarAsync(UsuarioId, id));
}
