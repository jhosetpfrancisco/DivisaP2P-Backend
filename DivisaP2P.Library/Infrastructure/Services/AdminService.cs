using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using DivisaP2P.Library.Core.Common;
using DivisaP2P.Library.Core.DTOs;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.Library.Infrastructure.Services;

/// <summary>
/// Panel administrativo (US-016) y reportes exportables (US-018). Consulta el
/// contexto directamente para las agregaciones de indicadores.
/// </summary>
public class AdminService : IAdminService
{
    private readonly DivisaP2PDbContext _context;

    public AdminService(DivisaP2PDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var ahora = DateTime.UtcNow;
        var inicioHoy = ahora.Date;
        var inicioMes = new DateTime(ahora.Year, ahora.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var hace30Dias = inicioHoy.AddDays(-29);

        var dashboard = new DashboardDto
        {
            UsuariosRegistrados = await _context.Usuarios.CountAsync(),
            OfertasActivas = await _context.Ofertas.CountAsync(o => o.Estado == EstadosOferta.Activa),
            TransaccionesHoy = await _context.Transacciones.CountAsync(t => t.FechaInicio >= inicioHoy),
            TransaccionesEnDisputa = await _context.Transacciones.CountAsync(t => t.Estado == EstadosTransaccion.EnDisputa),
            DisputasPendientes = await _context.Disputas.CountAsync(d => d.Estado == EstadosDisputa.Abierta)
        };

        // Volumen operado por divisa (transacciones completadas), agrupado por divisa origen.
        var completadas = await _context.Transacciones
            .Where(t => t.Estado == EstadosTransaccion.Completada)
            .Include(t => t.Oferta)
            .Select(t => new { t.Oferta.DivisaOrigen, t.MontoOperado, t.CompradorId, t.VendedorId, t.FechaActualizacion })
            .ToListAsync();

        dashboard.VolumenPorDivisa = completadas
            .GroupBy(t => t.DivisaOrigen)
            .Select(g => new VolumenPorDivisaDto { Divisa = g.Key, Volumen = g.Sum(x => x.MontoOperado) })
            .OrderByDescending(v => v.Volumen)
            .ToList();

        // Evolución diaria de transacciones de los últimos 30 días.
        var recientes = await _context.Transacciones
            .Where(t => t.FechaInicio >= hace30Dias)
            .Select(t => t.FechaInicio)
            .ToListAsync();

        dashboard.EvolucionDiaria = recientes
            .GroupBy(f => f.Date)
            .Select(g => new EvolucionDiariaDto { Fecha = g.Key, Cantidad = g.Count() })
            .OrderBy(e => e.Fecha)
            .ToList();

        // Top 10 de usuarios por volumen operado en el mes (como comprador o vendedor).
        var delMes = completadas.Where(t => t.FechaActualizacion >= inicioMes).ToList();
        var volumenPorUsuario = new Dictionary<int, decimal>();
        foreach (var t in delMes)
        {
            volumenPorUsuario[t.CompradorId] = volumenPorUsuario.GetValueOrDefault(t.CompradorId) + t.MontoOperado;
            volumenPorUsuario[t.VendedorId] = volumenPorUsuario.GetValueOrDefault(t.VendedorId) + t.MontoOperado;
        }

        var topIds = volumenPorUsuario.OrderByDescending(kv => kv.Value).Take(10).ToList();
        var nombres = await _context.Usuarios
            .Where(u => topIds.Select(k => k.Key).Contains(u.Id))
            .Select(u => new { u.Id, u.Nombres, u.RazonSocial })
            .ToListAsync();

        dashboard.TopUsuarios = topIds
            .Select(kv =>
            {
                var u = nombres.FirstOrDefault(n => n.Id == kv.Key);
                return new TopUsuarioDto
                {
                    UsuarioId = kv.Key,
                    Nombre = u?.RazonSocial ?? u?.Nombres ?? $"Usuario {kv.Key}",
                    VolumenOperado = kv.Value
                };
            })
            .ToList();

        return dashboard;
    }

    public async Task<ResultadoPaginado<TransaccionDto>> ReporteTransaccionesAsync(
        DateTime? desde, DateTime? hasta, string? estado, int pagina, int tamanioPagina)
    {
        var query = ConstruirQueryReporte(desde, hasta, estado);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.FechaInicio)
            .Skip((pagina - 1) * tamanioPagina)
            .Take(tamanioPagina)
            .ToListAsync();

        return new ResultadoPaginado<TransaccionDto>
        {
            Data = items.Select(Mapeos.ATransaccionDto),
            Total = total,
            Pagina = pagina,
            TamanioPagina = tamanioPagina
        };
    }

    public async Task<byte[]> ExportarTransaccionesCsvAsync(DateTime? desde, DateTime? hasta, string? estado)
    {
        var items = await ConstruirQueryReporte(desde, hasta, estado)
            .OrderByDescending(t => t.FechaInicio)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Codigo;FechaInicio;Comprador;Vendedor;DivisaOrigen;DivisaDestino;MontoOperado;TipoCambio;Estado");
        foreach (var t in items)
        {
            var comprador = t.Comprador?.RazonSocial ?? t.Comprador?.Nombres ?? "";
            var vendedor = t.Vendedor?.RazonSocial ?? t.Vendedor?.Nombres ?? "";
            sb.AppendLine(string.Join(';',
                t.Codigo,
                t.FechaInicio.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                comprador,
                vendedor,
                t.Oferta?.DivisaOrigen ?? "",
                t.Oferta?.DivisaDestino ?? "",
                t.MontoOperado.ToString(CultureInfo.InvariantCulture),
                t.TipoCambio.ToString(CultureInfo.InvariantCulture),
                t.Estado));
        }

        // BOM UTF-8 para que Excel respete los acentos.
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private IQueryable<Core.Entities.Transaccion> ConstruirQueryReporte(DateTime? desde, DateTime? hasta, string? estado)
    {
        var query = _context.Transacciones
            .Include(t => t.Oferta)
            .Include(t => t.Comprador)
            .Include(t => t.Vendedor)
            .AsNoTracking()
            .AsQueryable();

        if (desde.HasValue)
            query = query.Where(t => t.FechaInicio >= desde.Value);
        if (hasta.HasValue)
            query = query.Where(t => t.FechaInicio <= hasta.Value);
        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(t => t.Estado == estado);

        return query;
    }
}
