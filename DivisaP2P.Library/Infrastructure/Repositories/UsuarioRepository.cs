using Microsoft.EntityFrameworkCore;
using DivisaP2P.Library.Core.Entities;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.Library.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly DivisaP2PDbContext _context;

    public UsuarioRepository(DivisaP2PDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> GetByIdAsync(int id)
    {
        return await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario?> GetByCorreoAsync(string correo)
    {
        return await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);
    }

    public async Task<bool> ExisteCorreoAsync(string correo)
    {
        return await _context.Usuarios.AnyAsync(u => u.Correo == correo);
    }

    public async Task<bool> ExisteRucAsync(string ruc)
    {
        return await _context.Usuarios.AnyAsync(u => u.Ruc == ruc);
    }

    public async Task<Usuario> AddAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task UpdateAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<Usuario> Items, int Total)> BuscarAsync(
        string? rol, string? estado, decimal? calificacionMin, int pagina, int tamanioPagina)
    {
        var query = _context.Usuarios.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(rol))
            query = query.Where(u => u.Rol == rol);

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(u => u.Estado == estado);

        if (calificacionMin.HasValue)
            query = query.Where(u => u.CalificacionPromedio >= calificacionMin.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(u => u.FechaRegistro)
            .Skip((pagina - 1) * tamanioPagina)
            .Take(tamanioPagina)
            .ToListAsync();

        return (items, total);
    }
}
