using DivisaP2P.Library.Core.Common;
using DivisaP2P.Library.Core.DTOs;
using DivisaP2P.Library.Core.Entities;

namespace DivisaP2P.Library.Core.Interfaces;

// ---------- Calificación (US-012) ----------
public interface ICalificacionRepository
{
    Task<bool> YaCalificoAsync(int transaccionId, int calificadorId);
    Task<Calificacion> AddAsync(Calificacion calificacion);
    Task<(decimal Promedio, int Cantidad)> GetPromedioAsync(int calificadoId);
}

public interface ICalificacionService
{
    Task<ResultadoServicio<CalificacionDto>> CalificarAsync(int usuarioId, CalificacionCreateDto dto);
}

// ---------- Disputa (US-014, US-015) ----------
public interface IDisputaRepository
{
    Task<Disputa?> GetByIdAsync(int id);
    Task<Disputa?> GetByTransaccionAsync(int transaccionId);
    Task<Disputa> AddAsync(Disputa disputa);
    Task UpdateAsync(Disputa disputa);
    Task<(IEnumerable<Disputa> Items, int Total)> ListarAsync(string? estado, int pagina, int tamanioPagina);
}

public interface IDisputaService
{
    Task<ResultadoServicio<DisputaDto>> AbrirAsync(int usuarioId, DisputaCreateDto dto);
    Task<ResultadoPaginado<DisputaDto>> ListarAsync(string? estado, int pagina, int tamanioPagina);
    Task<DisputaDto?> GetByIdAsync(int id);
    Task<ResultadoServicio> ResolverAsync(int adminId, int disputaId, ResolucionDisputaDto dto);
}

// ---------- Notificación (US-021) ----------
public interface INotificacionRepository
{
    Task<IEnumerable<Notificacion>> GetByUsuarioAsync(int usuarioId, int dias);
    Task<int> ContarNoLeidasAsync(int usuarioId);
    Task<Notificacion?> GetByIdAsync(int id);
    Task AddAsync(Notificacion notificacion);
    Task UpdateAsync(Notificacion notificacion);
    Task MarcarTodasLeidasAsync(int usuarioId);
}

public interface INotificacionService
{
    Task<IEnumerable<NotificacionDto>> ListarAsync(int usuarioId);
    Task<int> ContarNoLeidasAsync(int usuarioId);
    Task<ResultadoServicio> MarcarLeidaAsync(int usuarioId, int notificacionId);
    Task MarcarTodasLeidasAsync(int usuarioId);
    /// <summary>Crea una notificación in-app (usado por los demás servicios).</summary>
    Task CrearAsync(int usuarioId, string titulo, string descripcion, string? enlace = null);
}

// ---------- Administración: dashboard y reportes (US-016, US-018) ----------
public interface IAdminService
{
    Task<DashboardDto> GetDashboardAsync();
    Task<ResultadoPaginado<TransaccionDto>> ReporteTransaccionesAsync(
        DateTime? desde, DateTime? hasta, string? estado, int pagina, int tamanioPagina);
    Task<byte[]> ExportarTransaccionesCsvAsync(DateTime? desde, DateTime? hasta, string? estado);
}
