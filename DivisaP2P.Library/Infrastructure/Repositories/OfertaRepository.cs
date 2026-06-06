using Microsoft.EntityFrameworkCore;
using DivisaP2P.Library.Core.Common;
using DivisaP2P.Library.Core.DTOs;
using DivisaP2P.Library.Core.Entities;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.Library.Infrastructure.Repositories;

public class OfertaRepository : IOfertaRepository
{
    private readonly DivisaP2PDbContext _context;

    public OfertaRepository(DivisaP2PDbContext context)
    {
        _context = context;
    }

    public async Task<Oferta?> GetByIdAsync(int id)
    {
        return await _context.Ofertas
            .Include(o => o.Usuario)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Oferta> AddAsync(Oferta oferta)
    {
        _context.Ofertas.Add(oferta);
        await _context.SaveChangesAsync();
        return oferta;
    }

    public async Task UpdateAsync(Oferta oferta)
    {
        _context.Ofertas.Update(oferta);
        await _context.SaveChangesAsync();
    }

    public async Task<int> ContarActivasPorUsuarioAsync(int usuarioId)
    {
        return await _context.Ofertas
            .CountAsync(o => o.UsuarioId == usuarioId && o.Estado == EstadosOferta.Activa);
    }

    public async Task<IEnumerable<Oferta>> GetPorUsuarioAsync(int usuarioId)
    {
        return await _context.Ofertas
            .Include(o => o.Usuario)
            .Where(o => o.UsuarioId == usuarioId)
            .OrderByDescending(o => o.FechaPublicacion)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<(IEnumerable<Oferta> Items, int Total)> BuscarAsync(OfertaFiltroDto filtro, int excluirUsuarioId)
    {
        var query = _context.Ofertas
            .Include(o => o.Usuario)
            .Where(o => o.Estado == EstadosOferta.Activa && o.UsuarioId != excluirUsuarioId)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro.TipoOperacion))
            query = query.Where(o => o.TipoOperacion == filtro.TipoOperacion);

        if (!string.IsNullOrWhiteSpace(filtro.Divisa))
            query = query.Where(o => o.DivisaOrigen == filtro.Divisa || o.DivisaDestino == filtro.Divisa);

        if (filtro.MontoMin.HasValue)
            query = query.Where(o => o.MontoDisponible >= filtro.MontoMin.Value);

        if (filtro.MontoMax.HasValue)
            query = query.Where(o => o.MontoDisponible <= filtro.MontoMax.Value);

        if (filtro.CalificacionMin.HasValue)
            query = query.Where(o => o.Usuario.CalificacionPromedio >= filtro.CalificacionMin.Value);

        query = filtro.OrdenarPor switch
        {
            "tipoCambio" => query.OrderByDescending(o => o.TipoCambio),
            "calificacion" => query.OrderByDescending(o => o.Usuario.CalificacionPromedio),
            _ => query.OrderByDescending(o => o.FechaPublicacion)
        };

        var total = await query.CountAsync();

        var items = await query
            .Skip((filtro.Pagina - 1) * filtro.TamanioPagina)
            .Take(filtro.TamanioPagina)
            .ToListAsync();

        return (items, total);
    }

    public async Task<IEnumerable<Oferta>> GetCandidatasMatchingAsync(Oferta ofertaBase)
    {
        // Una oferta de compra empareja con ofertas de venta de las mismas divisas
        // (origen/destino invertidos) y viceversa. Se excluyen las propias.
        var tipoContrario = ofertaBase.TipoOperacion == TipoOperacion.Compra
            ? TipoOperacion.Venta
            : TipoOperacion.Compra;

        return await _context.Ofertas
            .Include(o => o.Usuario)
            .Where(o => o.Estado == EstadosOferta.Activa
                        && o.UsuarioId != ofertaBase.UsuarioId
                        && o.TipoOperacion == tipoContrario
                        && o.DivisaOrigen == ofertaBase.DivisaDestino
                        && o.DivisaDestino == ofertaBase.DivisaOrigen
                        && o.MontoDisponible >= ofertaBase.MontoDisponible)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> ExpirarVencidasAsync(DateTime ahora)
    {
        var vencidas = await _context.Ofertas
            .Where(o => o.Estado == EstadosOferta.Activa && o.FechaExpiracion <= ahora)
            .ToListAsync();

        foreach (var oferta in vencidas)
            oferta.Estado = EstadosOferta.Expirada;

        if (vencidas.Count > 0)
            await _context.SaveChangesAsync();

        return vencidas.Count;
    }
}
