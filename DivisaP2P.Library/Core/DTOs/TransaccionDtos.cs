using System.ComponentModel.DataAnnotations;

namespace DivisaP2P.Library.Core.DTOs;

/// <summary>Transacción en listados e historial (US-011, US-013).</summary>
public class TransaccionDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = null!;
    public int OfertaId { get; set; }
    public int CompradorId { get; set; }
    public string CompradorNombre { get; set; } = null!;
    public int VendedorId { get; set; }
    public string VendedorNombre { get; set; } = null!;
    public string DivisaOrigen { get; set; } = null!;
    public string DivisaDestino { get; set; } = null!;
    public decimal MontoOperado { get; set; }
    public decimal TipoCambio { get; set; }
    public string Estado { get; set; } = null!;
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaLimiteAccion { get; set; }
}

/// <summary>Detalle de transacción con línea de tiempo y comprobantes (US-011).</summary>
public class TransaccionDetalleDto : TransaccionDto
{
    public List<HistorialEstadoDto> Historial { get; set; } = new();
    public List<VoucherDto> Vouchers { get; set; } = new();
}

public class HistorialEstadoDto
{
    public string Estado { get; set; } = null!;
    public string? Comentario { get; set; }
    public DateTime Fecha { get; set; }
}

public class VoucherDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = null!;
    public string NombreArchivo { get; set; } = null!;
    public string RutaArchivo { get; set; } = null!;
    public string NumeroOperacion { get; set; } = null!;
    public DateTime FechaDeposito { get; set; }
    public DateTime FechaSubida { get; set; }
}

/// <summary>Inicio de transacción a partir de una oferta (US-008).</summary>
public class IniciarTransaccionDto
{
    [Required]
    public int OfertaId { get; set; }

    [Required, Range(100, 500000)]
    public decimal Monto { get; set; }
}

/// <summary>Reporte de pago o entrega + datos del voucher (US-009).</summary>
public class ReporteDepositoDto
{
    [Required, MaxLength(50)]
    public string NumeroOperacion { get; set; } = null!;

    [Required]
    public DateTime FechaDeposito { get; set; }

    /// <summary>Nombre del archivo del voucher (la subida real del binario se simula
    /// guardando la ruta; en producción iría a almacenamiento de objetos).</summary>
    [Required, MaxLength(200)]
    public string NombreArchivo { get; set; } = null!;

    [Required, MaxLength(300)]
    public string RutaArchivo { get; set; } = null!;
}

/// <summary>Validación de un depósito reportado por la contraparte (US-010).</summary>
public class ValidacionDepositoDto
{
    [Required]
    public bool Aprobar { get; set; }

    /// <summary>Obligatorio (mín. 20 caracteres) cuando Aprobar = false.</summary>
    [MaxLength(500)]
    public string? MotivoRechazo { get; set; }
}
