using System;

namespace DivisaP2P.Library.Core.Entities;

/// <summary>
/// Cuenta bancaria registrada por un usuario. Es obligatorio tener al menos una
/// para poder publicar ofertas (US-003). Se permite una por divisa, con una marcada
/// como predeterminada por divisa.
/// </summary>
public partial class CuentaBancaria
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string Banco { get; set; } = null!;

    public string TipoCuenta { get; set; } = null!; // Ahorros / Corriente

    public string Divisa { get; set; } = null!; // PEN / USD / EUR

    public string NumeroCuenta { get; set; } = null!;

    public string Cci { get; set; } = null!; // 20 dígitos

    public string NombreTitular { get; set; } = null!;

    public bool EsPredeterminada { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
