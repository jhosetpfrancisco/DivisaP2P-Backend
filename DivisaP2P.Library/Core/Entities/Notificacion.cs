using System;

namespace DivisaP2P.Library.Core.Entities;

/// <summary>
/// Notificación in-app para un usuario (US-021). Accesible desde la campana del menú.
/// </summary>
public partial class Notificacion
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string Titulo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public string? Enlace { get; set; }

    public bool Leida { get; set; }

    public DateTime Fecha { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
