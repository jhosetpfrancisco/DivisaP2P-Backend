namespace DivisaP2P.Library.Core.Common;

/// <summary>
/// Envoltura estándar para respuestas paginadas (US-005, US-013, US-017).
/// </summary>
public class ResultadoPaginado<T>
{
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int TamanioPagina { get; set; }
    public int TotalPaginas => TamanioPagina > 0 ? (int)Math.Ceiling((double)Total / TamanioPagina) : 0;
}
