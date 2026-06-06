using DivisaP2P.Library.Core.Common;
using DivisaP2P.Library.Core.DTOs;
using DivisaP2P.Library.Core.Entities;

namespace DivisaP2P.Library.Core.Interfaces;

public interface ITransaccionRepository
{
    Task<Transaccion?> GetByIdAsync(int id);
    Task<Transaccion?> GetDetalleAsync(int id);
    Task<Transaccion> AddAsync(Transaccion transaccion);
    Task UpdateAsync(Transaccion transaccion);
    Task<bool> CodigoExisteAsync(string codigo);
    Task<(IEnumerable<Transaccion> Items, int Total)> GetHistorialAsync(
        int usuarioId, DateTime? desde, DateTime? hasta, string? estado, int pagina, int tamanioPagina);
}

public interface ITransaccionService
{
    Task<ResultadoServicio<TransaccionDto>> IniciarAsync(int usuarioId, IniciarTransaccionDto dto);
    Task<ResultadoServicio> ReportarDepositoAsync(int usuarioId, int transaccionId, ReporteDepositoDto dto);
    Task<ResultadoServicio> ValidarDepositoAsync(int usuarioId, int transaccionId, ValidacionDepositoDto dto);
    Task<ResultadoServicio<TransaccionDetalleDto>> GetDetalleAsync(int usuarioId, int transaccionId);
    Task<ResultadoPaginado<TransaccionDto>> HistorialAsync(
        int usuarioId, DateTime? desde, DateTime? hasta, string? estado, int pagina, int tamanioPagina);
}
