using System;

namespace DivisaP2P.Library.Core.Entities;

/// <summary>
/// Calificación de 1 a 5 estrellas que una parte da a la otra tras completar
/// una transacción (US-012). Cada usuario solo puede calificar una vez por transacción.
/// </summary>
public partial class Calificacion
{
    public int Id { get; set; }

    public int TransaccionId { get; set; }

    public int CalificadorId { get; set; }

    public int CalificadoId { get; set; }

    public int Estrellas { get; set; } // 1 a 5

    public string? Comentario { get; set; }

    public DateTime Fecha { get; set; }

    public virtual Transaccion Transaccion { get; set; } = null!;
}
