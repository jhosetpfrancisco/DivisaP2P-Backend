using Microsoft.EntityFrameworkCore;
using DivisaP2P.Library.Core.Common;
using DivisaP2P.Library.Core.Entities;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.Library.Infrastructure.Repositories;

public class CuentaBancariaRepository : ICuentaBancariaRepository
{
    private readonly DivisaP2PDbContext _context;

    public CuentaBancariaRepository(DivisaP2PDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CuentaBancaria>> GetByUsuarioAsync(int usuarioId)
    {
        return await _context.CuentasBancarias
            .Where(c => c.UsuarioId == usuarioId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<CuentaBancaria?> GetByIdAsync(int id)
    {
        return await _context.CuentasBancarias.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CuentaBancaria> AddAsync(CuentaBancaria cuenta)
    {
        _context.CuentasBancarias.Add(cuenta);
        await _context.SaveChangesAsync();
        return cuenta;
    }

    public async Task UpdateAsync(CuentaBancaria cuenta)
    {
        _context.CuentasBancarias.Update(cuenta);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(CuentaBancaria cuenta)
    {
        _context.CuentasBancarias.Remove(cuenta);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<CuentaBancaria>> GetPredeterminadasAsync(int usuarioId, string divisa)
    {
        return await _context.CuentasBancarias
            .Where(c => c.UsuarioId == usuarioId && c.Divisa == divisa && c.EsPredeterminada)
            .ToListAsync();
    }

    public async Task<bool> TieneOfertasActivasAsync(int cuentaId)
    {
        return await _context.Ofertas
            .AnyAsync(o => o.CuentaBancariaId == cuentaId && o.Estado == EstadosOferta.Activa);
    }
}
