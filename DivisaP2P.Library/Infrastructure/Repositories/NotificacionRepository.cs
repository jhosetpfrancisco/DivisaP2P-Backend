using Microsoft.EntityFrameworkCore;
using DivisaP2P.Library.Core.Entities;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.Library.Infrastructure.Repositories;

public class NotificacionRepository : INotificacionRepository
{
    private readonly DivisaP2PDbContext _context;

    public NotificacionRepository(DivisaP2PDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Notificacion>> GetByUsuarioAsync(int usuarioId, int dias)
    {
        // El historial conserva las notificaciones de los últimos N días (US-021).
        var limite = DateTime.UtcNow.AddDays(-dias);
        return await _context.Notificaciones
            .Where(n => n.UsuarioId == usuarioId && n.Fecha >= limite)
            .OrderByDescending(n => n.Fecha)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> ContarNoLeidasAsync(int usuarioId)
    {
        return await _context.Notificaciones
            .CountAsync(n => n.UsuarioId == usuarioId && !n.Leida);
    }

    public async Task<Notificacion?> GetByIdAsync(int id)
    {
        return await _context.Notificaciones.FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task AddAsync(Notificacion notificacion)
    {
        _context.Notificaciones.Add(notificacion);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Notificacion notificacion)
    {
        _context.Notificaciones.Update(notificacion);
        await _context.SaveChangesAsync();
    }

    public async Task MarcarTodasLeidasAsync(int usuarioId)
    {
        await _context.Notificaciones
            .Where(n => n.UsuarioId == usuarioId && !n.Leida)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.Leida, true));
    }
}
