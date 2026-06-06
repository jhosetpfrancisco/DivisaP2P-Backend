using System.ComponentModel.DataAnnotations;

namespace DivisaP2P.Library.Core.DTOs;

/// <summary>Registro de usuario persona natural (US-001).</summary>
public class RegistroUsuarioDto
{
    [Required, MaxLength(100)]
    public string Nombres { get; set; } = null!;

    [Required, MaxLength(60)]
    public string ApellidoPaterno { get; set; } = null!;

    [Required, MaxLength(60)]
    public string ApellidoMaterno { get; set; } = null!;

    [Required, EmailAddress, MaxLength(120)]
    public string Correo { get; set; } = null!;

    /// <summary>Mínimo 8, una mayúscula, una minúscula y un número.</summary>
    [Required]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
        ErrorMessage = "La contraseña debe tener mínimo 8 caracteres, una mayúscula, una minúscula y un número.")]
    public string Password { get; set; } = null!;

    [Required, RegularExpression("DNI|CE", ErrorMessage = "El tipo de documento debe ser DNI o CE.")]
    public string TipoDocumento { get; set; } = null!;

    [Required, MaxLength(12)]
    public string NumeroDocumento { get; set; } = null!;

    [Required, MaxLength(20)]
    public string Celular { get; set; } = null!;

    [Required(ErrorMessage = "Debe aceptar los términos y condiciones.")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "Debe aceptar los términos y condiciones.")]
    public bool AceptaTerminos { get; set; }
}

/// <summary>Registro de Empresa de Turismo (US-019).</summary>
public class RegistroEmpresaDto
{
    [Required, MaxLength(150)]
    public string RazonSocial { get; set; } = null!;

    [Required]
    [RegularExpression(@"^(10|15|17|20)\d{9}$",
        ErrorMessage = "El RUC debe tener 11 dígitos y comenzar con 10, 15, 17 o 20.")]
    public string Ruc { get; set; } = null!;

    [Required, MaxLength(150)]
    public string RepresentanteLegal { get; set; } = null!;

    [Required, EmailAddress, MaxLength(120)]
    public string Correo { get; set; } = null!;

    [Required]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
        ErrorMessage = "La contraseña debe tener mínimo 8 caracteres, una mayúscula, una minúscula y un número.")]
    public string Password { get; set; } = null!;

    [Required, MaxLength(20)]
    public string Celular { get; set; } = null!;
}

/// <summary>Inicio de sesión (US-002).</summary>
public class LoginDto
{
    [Required, EmailAddress]
    public string Correo { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}

/// <summary>Respuesta de autenticación: token JWT + datos básicos del usuario.</summary>
public class AuthResponseDto
{
    public string Token { get; set; } = null!;
    public DateTime ExpiraEn { get; set; }
    public int UsuarioId { get; set; }
    public string Rol { get; set; } = null!;
    public string NombreMostrado { get; set; } = null!;
    public string Correo { get; set; } = null!;
}
