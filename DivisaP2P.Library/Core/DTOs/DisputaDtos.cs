using System.ComponentModel.DataAnnotations;

namespace DivisaP2P.Library.Core.DTOs;

public class DisputaDto
{
    public int Id { get; set; }
    public int TransaccionId { get; set; }
    public string TransaccionCodigo { get; set; } = null!;
    public int AbiertaPorId { get; set; }
    public string Motivo { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public string? Resolucion { get; set; }
    public string? ComentarioResolucion { get; set; }
    public DateTime FechaApertura { get; set; }
    public DateTime? FechaResolucion { get; set; }
    public List<string> Evidencias { get; set; } = new();
}

/// <summary>Apertura de disputa (US-014).</summary>
public class DisputaCreateDto
{
    [Required]
    public int TransaccionId { get; set; }

    [Required, MinLength(20, ErrorMessage = "El motivo debe tener al menos 20 caracteres.")]
    [MaxLength(1000)]
    public string Motivo { get; set; } = null!;

    /// <summary>Hasta 3 rutas de evidencia (JPG/PNG/PDF).</summary>
    [MaxLength(3, ErrorMessage = "Se permiten máximo 3 evidencias.")]
    public List<string> Evidencias { get; set; } = new();
}

/// <summary>Resolución de disputa por el administrador (US-015).</summary>
public class ResolucionDisputaDto
{
    [Required, RegularExpression("AFavorComprador|AFavorVendedor|Anulada",
        ErrorMessage = "La resolución debe ser AFavorComprador, AFavorVendedor o Anulada.")]
    public string Resolucion { get; set; } = null!;

    [Required, MinLength(50, ErrorMessage = "El comentario debe tener al menos 50 caracteres.")]
    [MaxLength(1000)]
    public string Comentario { get; set; } = null!;
}
