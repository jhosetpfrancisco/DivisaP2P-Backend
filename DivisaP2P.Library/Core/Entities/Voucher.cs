using System;

namespace DivisaP2P.Library.Core.Entities;

/// <summary>
/// Comprobante de transferencia subido por una de las partes (US-009).
/// Tipo distingue si corresponde al pago (comprador) o a la entrega (vendedor).
/// </summary>
public partial class Voucher
{
    public int Id { get; set; }

    public int TransaccionId { get; set; }

    public int UsuarioId { get; set; }

    public string Tipo { get; set; } = null!; // Pago / Entrega

    public string RutaArchivo { get; set; } = null!;

    public string NombreArchivo { get; set; } = null!;

    public string NumeroOperacion { get; set; } = null!;

    public DateTime FechaDeposito { get; set; }

    public DateTime FechaSubida { get; set; }

    public virtual Transaccion Transaccion { get; set; } = null!;
}
