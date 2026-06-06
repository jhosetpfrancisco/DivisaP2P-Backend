using System.ComponentModel.DataAnnotations;

namespace DivisaP2P.Library.Core.DTOs;

/// <summary>Datos de perfil que la API devuelve (US-003).</summary>
public class UsuarioDto
{
    public int Id { get; set; }
    public string Rol { get; set; } = null!;
    public string Nombres { get; set; } = null!;
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public string? RazonSocial { get; set; }
    public string? Ruc { get; set; }
    public string Correo { get; set; } = null!;
    public string? TipoDocumento { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? Celular { get; set; }
    public string Estado { get; set; } = null!;
    public bool CorreoVerificado { get; set; }
    public decimal CalificacionPromedio { get; set; }
    public int OperacionesCompletadas { get; set; }
    public DateTime FechaRegistro { get; set; }
}

/// <summary>Actualización de perfil. Correo y documento no son editables (US-003).</summary>
public class PerfilUpdateDto
{
    [Required, MaxLength(100)]
    public string Nombres { get; set; } = null!;

    [MaxLength(60)]
    public string? ApellidoPaterno { get; set; }

    [MaxLength(60)]
    public string? ApellidoMaterno { get; set; }

    [Required, MaxLength(20)]
    public string Celular { get; set; } = null!;
}

/// <summary>Cambio de contraseña: requiere la contraseña actual (US-003).</summary>
public class CambioPasswordDto
{
    [Required]
    public string PasswordActual { get; set; } = null!;

    [Required]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
        ErrorMessage = "La contraseña debe tener mínimo 8 caracteres, una mayúscula, una minúscula y un número.")]
    public string PasswordNueva { get; set; } = null!;
}

/// <summary>Bloqueo/desbloqueo de cuenta por el administrador (US-017).</summary>
public class BloqueoUsuarioDto
{
    [Required, MinLength(30, ErrorMessage = "El motivo debe tener al menos 30 caracteres.")]
    [MaxLength(300)]
    public string Motivo { get; set; } = null!;
}
