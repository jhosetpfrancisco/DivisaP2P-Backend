using System;
using System.Collections.Generic;

namespace DivisaP2P.Library.Core.Entities;

/// <summary>
/// Usuario de la plataforma. Un mismo modelo cubre los tres roles del proyecto:
/// USU (persona natural), ETU (empresa de turismo) y ADM (administrador).
/// Los campos corporativos (RazonSocial, Ruc, RepresentanteLegal) solo aplican a ETU.
/// </summary>
public partial class Usuario
{
    public int Id { get; set; }

    public string Rol { get; set; } = null!;

    public string Nombres { get; set; } = null!;

    public string? ApellidoPaterno { get; set; }

    public string? ApellidoMaterno { get; set; }

    // Datos corporativos (solo ETU)
    public string? RazonSocial { get; set; }

    public string? Ruc { get; set; }

    public string? RepresentanteLegal { get; set; }

    public string Correo { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? TipoDocumento { get; set; }

    public string? NumeroDocumento { get; set; }

    public string? Celular { get; set; }

    public string Estado { get; set; } = null!;

    public bool CorreoVerificado { get; set; }

    public decimal CalificacionPromedio { get; set; }

    public int OperacionesCompletadas { get; set; }

    public int IntentosFallidos { get; set; }

    public DateTime? BloqueadoHasta { get; set; }

    public string? MotivoBloqueo { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual ICollection<CuentaBancaria> CuentasBancarias { get; set; } = new List<CuentaBancaria>();

    public virtual ICollection<Oferta> Ofertas { get; set; } = new List<Oferta>();

    public virtual ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
}
