using Microsoft.EntityFrameworkCore;
using DivisaP2P.Library.Core.Entities;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.Library.Infrastructure.Repositories;

public class DisputaRepository : IDisputaRepository
{
    private readonly DivisaP2PDbContext _context;

    public DisputaRepository(DivisaP2PDbContext context)
    {
        _context = context;
    }

    public async Task<Disputa?> GetByIdAsync(int id)
    {
        return await _context.Disputas
            .Include(d => d.Evidencias)
            .Include(d => d.Transaccion)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Disputa?> GetByTransaccionAsync(int transaccionId)
    {
        return await _context.Disputas
            .FirstOrDefaultAsync(d => d.TransaccionId == transaccionId);
    }

    public async Task<Disputa> AddAsync(Disputa disputa)
    {
        _context.Disputas.Add(disputa);
        await _context.SaveChangesAsync();
        return disputa;
    }

    public async Task UpdateAsync(Disputa disputa)
    {
        _context.Disputas.Update(disputa);
        await _context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<Disputa> Items, int Total)> ListarAsync(string? estado, int pagina, int tamanioPagina)
    {
        var query = _context.Disputas
            .Include(d => d.Evidencias)
            .Include(d => d.Transaccion)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(d => d.Estado == estado);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(d => d.FechaApertura)
            .Skip((pagina - 1) * tamanioPagina)
            .Take(tamanioPagina)
            .ToListAsync();

        return (items, total);
    }
}
