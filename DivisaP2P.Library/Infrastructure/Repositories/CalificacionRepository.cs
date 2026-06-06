using Microsoft.EntityFrameworkCore;
using DivisaP2P.Library.Core.Entities;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.Library.Infrastructure.Repositories;

public class CalificacionRepository : ICalificacionRepository
{
    private readonly DivisaP2PDbContext _context;

    public CalificacionRepository(DivisaP2PDbContext context)
    {
        _context = context;
    }

    public async Task<bool> YaCalificoAsync(int transaccionId, int calificadorId)
    {
        return await _context.Calificaciones
            .AnyAsync(c => c.TransaccionId == transaccionId && c.CalificadorId == calificadorId);
    }

    public async Task<Calificacion> AddAsync(Calificacion calificacion)
    {
        _context.Calificaciones.Add(calificacion);
        await _context.SaveChangesAsync();
        return calificacion;
    }

    public async Task<(decimal Promedio, int Cantidad)> GetPromedioAsync(int calificadoId)
    {
        var calificaciones = await _context.Calificaciones
            .Where(c => c.CalificadoId == calificadoId)
            .Select(c => c.Estrellas)
            .ToListAsync();

        if (calificaciones.Count == 0)
            return (0m, 0);

        return ((decimal)calificaciones.Average(), calificaciones.Count);
    }
}
