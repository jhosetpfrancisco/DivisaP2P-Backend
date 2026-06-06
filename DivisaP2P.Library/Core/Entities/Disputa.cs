using System;
using System.Collections.Generic;

namespace DivisaP2P.Library.Core.Entities;

/// <summary>
/// Disputa abierta sobre una transacción (US-014). Congela la transacción hasta
/// que el administrador emita una resolución (US-015). Una transacción admite
/// como máximo una disputa.
/// </summary>
public partial class Disputa
{
    public int Id { get; set; }

    public int TransaccionId { get; set; }

    public int AbiertaPorId { get; set; }

    public string Motivo { get; set; } = null!;

    public string Estado { get; set; } = null!; // Abierta / Resuelta

    public string? Resolucion { get; set; } // AFavorComprador / AFavorVendedor / Anulada

    public string? ComentarioResolucion { get; set; }

    public DateTime FechaApertura { get; set; }

    public DateTime? FechaResolucion { get; set; }

    public virtual Transaccion Transaccion { get; set; } = null!;

    public virtual ICollection<DisputaEvidencia> Evidencias { get; set; } = new List<DisputaEvidencia>();
}
