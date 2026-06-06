using System.ComponentModel.DataAnnotations;

namespace DivisaP2P.Library.Core.DTOs;

/// <summary>Oferta tal como se devuelve en listados y detalle (US-004, US-005).</summary>
public class OfertaDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = null!;
    public decimal UsuarioCalificacion { get; set; }
    public string TipoOperacion { get; set; } = null!;
    public string DivisaOrigen { get; set; } = null!;
    public string DivisaDestino { get; set; } = null!;
    public decimal MontoTotal { get; set; }
    public decimal MontoDisponible { get; set; }
    public decimal TipoCambio { get; set; }
    public string Estado { get; set; } = null!;
    public bool EsVolumenEtu { get; set; }
    public DateTime FechaPublicacion { get; set; }
    public DateTime FechaExpiracion { get; set; }
}

/// <summary>Publicación de oferta (US-004).</summary>
public class OfertaCreateDto
{
    [Required, RegularExpression("Compra|Venta", ErrorMessage = "El tipo de operación debe ser Compra o Venta.")]
    public string TipoOperacion { get; set; } = null!;

    [Required, RegularExpression("PEN|USD|EUR")]
    public string DivisaOrigen { get; set; } = null!;

    [Required, RegularExpression("PEN|USD|EUR")]
    public string DivisaDestino { get; set; } = null!;

    [Required, Range(100, 500000, ErrorMessage = "El monto debe estar entre 100 y 500000.")]
    public decimal Monto { get; set; }

    [Required, Range(0.0001, double.MaxValue, ErrorMessage = "El tipo de cambio debe ser un valor positivo.")]
    public decimal TipoCambio { get; set; }

    [Required]
    public int CuentaBancariaId { get; set; }
}

/// <summary>Edición de oferta: solo se permite cambiar el tipo de cambio (US-006).</summary>
public class OfertaUpdateDto
{
    [Required, Range(0.0001, double.MaxValue, ErrorMessage = "El tipo de cambio debe ser un valor positivo.")]
    public decimal TipoCambio { get; set; }
}

/// <summary>Filtros de búsqueda de ofertas (US-005). Llega como query string.</summary>
public class OfertaFiltroDto
{
    public string? TipoOperacion { get; set; }
    public string? Divisa { get; set; }
    public decimal? MontoMin { get; set; }
    public decimal? MontoMax { get; set; }
    public decimal? CalificacionMin { get; set; }
    /// <summary>tipoCambio | calificacion | recientes</summary>
    public string OrdenarPor { get; set; } = "recientes";
    public int Pagina { get; set; } = 1;
    public int TamanioPagina { get; set; } = 10;
}
