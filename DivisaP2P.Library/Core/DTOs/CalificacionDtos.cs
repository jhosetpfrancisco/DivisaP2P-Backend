using System.ComponentModel.DataAnnotations;

namespace DivisaP2P.Library.Core.DTOs;

public class CalificacionDto
{
    public int Id { get; set; }
    public int TransaccionId { get; set; }
    public int CalificadorId { get; set; }
    public int CalificadoId { get; set; }
    public int Estrellas { get; set; }
    public string? Comentario { get; set; }
    public DateTime Fecha { get; set; }
}

/// <summary>Calificación de la contraparte tras una transacción completada (US-012).</summary>
public class CalificacionCreateDto
{
    [Required]
    public int TransaccionId { get; set; }

    [Required, Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5 estrellas.")]
    public int Estrellas { get; set; }

    [MaxLength(200)]
    public string? Comentario { get; set; }
}
