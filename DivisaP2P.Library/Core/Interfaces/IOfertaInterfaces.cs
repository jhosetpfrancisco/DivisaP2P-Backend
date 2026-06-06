using DivisaP2P.Library.Core.Common;
using DivisaP2P.Library.Core.DTOs;
using DivisaP2P.Library.Core.Entities;

namespace DivisaP2P.Library.Core.Interfaces;

public interface IOfertaRepository
{
    Task<Oferta?> GetByIdAsync(int id);
    Task<Oferta> AddAsync(Oferta oferta);
    Task UpdateAsync(Oferta oferta);
    Task<int> ContarActivasPorUsuarioAsync(int usuarioId);
    Task<IEnumerable<Oferta>> GetPorUsuarioAsync(int usuarioId);
    Task<(IEnumerable<Oferta> Items, int Total)> BuscarAsync(OfertaFiltroDto filtro, int excluirUsuarioId);
    /// <summary>Ofertas activas candidatas a hacer match con la oferta indicada (US-007).</summary>
    Task<IEnumerable<Oferta>> GetCandidatasMatchingAsync(Oferta ofertaBase);
    /// <summary>Expira en bloque las ofertas activas vencidas (RN-006). Devuelve cuántas expiró.</summary>
    Task<int> ExpirarVencidasAsync(DateTime ahora);
}

public interface IOfertaService
{
    Task<ResultadoServicio<OfertaDto>> PublicarAsync(int usuarioId, string rol, OfertaCreateDto dto);
    Task<ResultadoPaginado<OfertaDto>> BuscarAsync(OfertaFiltroDto filtro, int usuarioId);
    Task<OfertaDto?> GetByIdAsync(int id);
    Task<IEnumerable<OfertaDto>> MisOfertasAsync(int usuarioId);
    Task<ResultadoServicio> ActualizarTipoCambioAsync(int usuarioId, int ofertaId, OfertaUpdateDto dto);
    Task<ResultadoServicio> CancelarAsync(int usuarioId, int ofertaId);
    Task<ResultadoServicio<IEnumerable<OfertaDto>>> MatchingAsync(int usuarioId, int ofertaId);
}
