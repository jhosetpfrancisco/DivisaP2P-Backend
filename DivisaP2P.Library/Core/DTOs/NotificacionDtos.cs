namespace DivisaP2P.Library.Core.DTOs;

public class NotificacionDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public string? Enlace { get; set; }
    public bool Leida { get; set; }
    public DateTime Fecha { get; set; }
}
