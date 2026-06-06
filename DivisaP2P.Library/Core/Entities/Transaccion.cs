using System;
using System.Collections.Generic;

namespace DivisaP2P.Library.Core.Entities;

/// <summary>
/// Operación concreta de intercambio entre dos usuarios, derivada de una oferta
/// tomada (US-008). El comprador entrega la divisa origen; el vendedor entrega la
/// divisa destino. El campo Estado avanza por la máquina de estados del proyecto.
/// </summary>
public partial class Transaccion
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!; // TXN-XXXXX

    public int OfertaId { get; set; }

    public int CompradorId { get; set; }

    public int VendedorId { get; set; }

    public decimal MontoOperado { get; set; }

    public decimal TipoCambio { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime FechaInicio { get; set; }

    /// <summary>Fecha límite para la siguiente acción esperada (pago, validación, etc.).</summary>
    public DateTime? FechaLimiteAccion { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public virtual Oferta Oferta { get; set; } = null!;

    public virtual Usuario Comprador { get; set; } = null!;

    public virtual Usuario Vendedor { get; set; } = null!;

    public virtual ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();

    public virtual ICollection<HistorialEstadoTransaccion> Historial { get; set; } = new List<HistorialEstadoTransaccion>();

    public virtual ICollection<Calificacion> Calificaciones { get; set; } = new List<Calificacion>();

    public virtual Disputa? Disputa { get; set; }
}
