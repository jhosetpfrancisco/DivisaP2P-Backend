using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DivisaP2P.Library.Core.DTOs;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.WebApi.Controllers;

/// <summary>Ciclo de vida de transacciones e historial (US-008 a US-011, US-013).</summary>
[Authorize]
public class TransaccionesController : ApiControllerBase
{
    private readonly ITransaccionService _transacciones;

    public TransaccionesController(ITransaccionService transacciones)
    {
        _transacciones = transacciones;
    }

    // POST: api/transacciones  (iniciar transacción a partir de una oferta)
    [HttpPost]
    public async Task<IActionResult> Iniciar(IniciarTransaccionDto dto) =>
        Responder(await _transacciones.IniciarAsync(UsuarioId, dto));

    // POST: api/transacciones/5/reportar  (reportar pago o entrega + voucher)
    [HttpPost("{id:int}/reportar")]
    public async Task<IActionResult> Reportar(int id, ReporteDepositoDto dto) =>
        Responder(await _transacciones.ReportarDepositoAsync(UsuarioId, id, dto));

    // POST: api/transacciones/5/validar  (confirmar o rechazar el depósito)
    [HttpPost("{id:int}/validar")]
    public async Task<IActionResult> Validar(int id, ValidacionDepositoDto dto) =>
        Responder(await _transacciones.ValidarDepositoAsync(UsuarioId, id, dto));

    // GET: api/transacciones/5  (detalle con línea de tiempo)
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detalle(int id) =>
        Responder(await _transacciones.GetDetalleAsync(UsuarioId, id));

    // GET: api/transacciones?desde=2026-01-01&estado=Completada&pagina=1
    [HttpGet]
    public async Task<IActionResult> Historial(
        [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, [FromQuery] string? estado,
        [FromQuery] int pagina = 1, [FromQuery] int tamanioPagina = 20) =>
        Ok(await _transacciones.HistorialAsync(UsuarioId, desde, hasta, estado, pagina, tamanioPagina));
}
