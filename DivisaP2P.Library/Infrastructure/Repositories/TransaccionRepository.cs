using Microsoft.EntityFrameworkCore;
using DivisaP2P.Library.Core.Entities;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.Library.Infrastructure.Repositories;

public class TransaccionRepository : ITransaccionRepository
{
    private readonly DivisaP2PDbContext _context;

    public TransaccionRepository(DivisaP2PDbContext context)
    {
        _context = context;
    }

    public async Task<Transaccion?> GetByIdAsync(int id)
    {
        return await _context.Transacciones
            .Include(t => t.Oferta)
            .Include(t => t.Comprador)
            .Include(t => t.Vendedor)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Transaccion?> GetDetalleAsync(int id)
    {
        return await _context.Transacciones
            .Include(t => t.Oferta)
            .Include(t => t.Comprador)
            .Include(t => t.Vendedor)
            .Include(t => t.Vouchers)
            .Include(t => t.Historial)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Transaccion> AddAsync(Transaccion transaccion)
    {
        _context.Transacciones.Add(transaccion);
        await _context.SaveChangesAsync();
        return transaccion;
    }

    public async Task UpdateAsync(Transaccion transaccion)
    {
        _context.Transacciones.Update(transaccion);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> CodigoExisteAsync(string codigo)
    {
        return await _context.Transacciones.AnyAsync(t => t.Codigo == codigo);
    }

    public async Task<(IEnumerable<Transaccion> Items, int Total)> GetHistorialAsync(
        int usuarioId, DateTime? desde, DateTime? hasta, string? estado, int pagina, int tamanioPagina)
    {
        var query = _context.Transacciones
            .Include(t => t.Oferta)
            .Include(t => t.Comprador)
            .Include(t => t.Vendedor)
            .Where(t => t.CompradorId == usuarioId || t.VendedorId == usuarioId)
            .AsNoTracking()
            .AsQueryable();

        if (desde.HasValue)
            query = query.Where(t => t.FechaInicio >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(t => t.FechaInicio <= hasta.Value);

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(t => t.Estado == estado);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.FechaInicio)
            .Skip((pagina - 1) * tamanioPagina)
            .Take(tamanioPagina)
            .ToListAsync();

        return (items, total);
    }
}
