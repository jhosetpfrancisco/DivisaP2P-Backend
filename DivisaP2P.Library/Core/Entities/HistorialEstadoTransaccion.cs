using System;

namespace DivisaP2P.Library.Core.Entities;

/// <summary>
/// Línea de tiempo de una transacción (US-011): cada cambio de estado deja
/// un registro con fecha/hora y un comentario opcional para auditoría.
/// </summary>
public partial class HistorialEstadoTransaccion
{
    public int Id { get; set; }

    public int TransaccionId { get; set; }

    public string Estado { get; set; } = null!;

    public string? Comentario { get; set; }

    public DateTime Fecha { get; set; }

    public virtual Transaccion Transaccion { get; set; } = null!;
}
