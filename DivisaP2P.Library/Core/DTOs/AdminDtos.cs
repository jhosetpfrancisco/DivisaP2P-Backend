namespace DivisaP2P.Library.Core.DTOs;

/// <summary>Indicadores del panel administrativo (US-016).</summary>
public class DashboardDto
{
    public int UsuariosRegistrados { get; set; }
    public int OfertasActivas { get; set; }
    public int TransaccionesHoy { get; set; }
    public int TransaccionesEnDisputa { get; set; }
    public List<VolumenPorDivisaDto> VolumenPorDivisa { get; set; } = new();
    public List<EvolucionDiariaDto> EvolucionDiaria { get; set; } = new();
    public List<TopUsuarioDto> TopUsuarios { get; set; } = new();
    public int DisputasPendientes { get; set; }
}

public class VolumenPorDivisaDto
{
    public string Divisa { get; set; } = null!;
    public decimal Volumen { get; set; }
}

public class EvolucionDiariaDto
{
    public DateTime Fecha { get; set; }
    public int Cantidad { get; set; }
}

public class TopUsuarioDto
{
    public int UsuarioId { get; set; }
    public string Nombre { get; set; } = null!;
    public decimal VolumenOperado { get; set; }
}

/// <summary>Fila genérica de reporte exportable (US-018).</summary>
public class ReporteFilaDto
{
    public Dictionary<string, object?> Campos { get; set; } = new();
}
