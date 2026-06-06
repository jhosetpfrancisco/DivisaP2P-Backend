using System;

namespace DivisaP2P.Library.Core.Entities;

/// <summary>
/// Archivo de evidencia adjunto a una disputa (hasta 3 por disputa según US-014).
/// </summary>
public partial class DisputaEvidencia
{
    public int Id { get; set; }

    public int DisputaId { get; set; }

    public string RutaArchivo { get; set; } = null!;

    public string NombreArchivo { get; set; } = null!;

    public virtual Disputa Disputa { get; set; } = null!;
}
