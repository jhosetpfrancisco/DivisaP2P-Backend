using System.ComponentModel.DataAnnotations;

namespace DivisaP2P.Library.Core.DTOs;

public class CuentaBancariaDto
{
    public int Id { get; set; }
    public string Banco { get; set; } = null!;
    public string TipoCuenta { get; set; } = null!;
    public string Divisa { get; set; } = null!;
    public string NumeroCuenta { get; set; } = null!;
    public string Cci { get; set; } = null!;
    public string NombreTitular { get; set; } = null!;
    public bool EsPredeterminada { get; set; }
}

/// <summary>Alta de cuenta bancaria del usuario (US-003).</summary>
public class CuentaBancariaCreateDto
{
    [Required, MaxLength(80)]
    public string Banco { get; set; } = null!;

    [Required, RegularExpression("Ahorros|Corriente", ErrorMessage = "El tipo de cuenta debe ser Ahorros o Corriente.")]
    public string TipoCuenta { get; set; } = null!;

    [Required, RegularExpression("PEN|USD|EUR", ErrorMessage = "La divisa debe ser PEN, USD o EUR.")]
    public string Divisa { get; set; } = null!;

    [Required, MaxLength(30)]
    public string NumeroCuenta { get; set; } = null!;

    [Required, RegularExpression(@"^\d{20}$", ErrorMessage = "El CCI debe tener exactamente 20 dígitos.")]
    public string Cci { get; set; } = null!;

    [Required, MaxLength(150)]
    public string NombreTitular { get; set; } = null!;

    public bool EsPredeterminada { get; set; }
}
