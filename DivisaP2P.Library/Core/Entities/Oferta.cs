using System;
using System.Collections.Generic;

namespace DivisaP2P.Library.Core.Entities;

/// <summary>
/// Oferta de compra o venta de divisas publicada por un usuario (US-004).
/// MontoDisponible parte igual a MontoTotal y se va descontando cuando otros
/// usuarios inician transacciones parciales sobre la oferta.
/// </summary>
public partial class Oferta
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string TipoOperacion { get; set; } = null!; // Compra / Venta

    public string DivisaOrigen { get; set; } = null!;

    public string DivisaDestino { get; set; } = null!;

    public decimal MontoTotal { get; set; }

    public decimal MontoDisponible { get; set; }

    public decimal TipoCambio { get; set; }

    public string Estado { get; set; } = null!; // Activa / Expirada / Agotada / Cancelada

    public int CuentaBancariaId { get; set; }

    public bool EsVolumenEtu { get; set; }

    public DateTime FechaPublicacion { get; set; }

    public DateTime FechaExpiracion { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;

    public virtual CuentaBancaria CuentaBancaria { get; set; } = null!;

    public virtual ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
}
