using DivisaP2P.Library.Core.DTOs;
using DivisaP2P.Library.Core.Entities;

namespace DivisaP2P.Library.Infrastructure.Services;

/// <summary>
/// Conversión entidad -> DTO centralizada, reutilizada por los servicios para
/// no repetir el mapeo en cada uno.
/// </summary>
public static class Mapeos
{
    public static string NombreMostrado(Usuario u) => u.RazonSocial ?? u.Nombres;

    public static UsuarioDto AUsuarioDto(Usuario u) => new()
    {
        Id = u.Id,
        Rol = u.Rol,
        Nombres = u.Nombres,
        ApellidoPaterno = u.ApellidoPaterno,
        ApellidoMaterno = u.ApellidoMaterno,
        RazonSocial = u.RazonSocial,
        Ruc = u.Ruc,
        Correo = u.Correo,
        TipoDocumento = u.TipoDocumento,
        NumeroDocumento = u.NumeroDocumento,
        Celular = u.Celular,
        Estado = u.Estado,
        CorreoVerificado = u.CorreoVerificado,
        CalificacionPromedio = u.CalificacionPromedio,
        OperacionesCompletadas = u.OperacionesCompletadas,
        FechaRegistro = u.FechaRegistro
    };

    public static CuentaBancariaDto ACuentaBancariaDto(CuentaBancaria c) => new()
    {
        Id = c.Id,
        Banco = c.Banco,
        TipoCuenta = c.TipoCuenta,
        Divisa = c.Divisa,
        NumeroCuenta = c.NumeroCuenta,
        Cci = c.Cci,
        NombreTitular = c.NombreTitular,
        EsPredeterminada = c.EsPredeterminada
    };

    public static OfertaDto AOfertaDto(Oferta o) => new()
    {
        Id = o.Id,
        UsuarioId = o.UsuarioId,
        UsuarioNombre = o.Usuario is null ? string.Empty : NombreMostrado(o.Usuario),
        UsuarioCalificacion = o.Usuario?.CalificacionPromedio ?? 0m,
        TipoOperacion = o.TipoOperacion,
        DivisaOrigen = o.DivisaOrigen,
        DivisaDestino = o.DivisaDestino,
        MontoTotal = o.MontoTotal,
        MontoDisponible = o.MontoDisponible,
        TipoCambio = o.TipoCambio,
        Estado = o.Estado,
        EsVolumenEtu = o.EsVolumenEtu,
        FechaPublicacion = o.FechaPublicacion,
        FechaExpiracion = o.FechaExpiracion
    };

    public static TransaccionDto ATransaccionDto(Transaccion t) => new()
    {
        Id = t.Id,
        Codigo = t.Codigo,
        OfertaId = t.OfertaId,
        CompradorId = t.CompradorId,
        CompradorNombre = t.Comprador is null ? string.Empty : NombreMostrado(t.Comprador),
        VendedorId = t.VendedorId,
        VendedorNombre = t.Vendedor is null ? string.Empty : NombreMostrado(t.Vendedor),
        DivisaOrigen = t.Oferta?.DivisaOrigen ?? string.Empty,
        DivisaDestino = t.Oferta?.DivisaDestino ?? string.Empty,
        MontoOperado = t.MontoOperado,
        TipoCambio = t.TipoCambio,
        Estado = t.Estado,
        FechaInicio = t.FechaInicio,
        FechaLimiteAccion = t.FechaLimiteAccion
    };

    public static TransaccionDetalleDto ATransaccionDetalleDto(Transaccion t)
    {
        var dto = new TransaccionDetalleDto
        {
            Id = t.Id,
            Codigo = t.Codigo,
            OfertaId = t.OfertaId,
            CompradorId = t.CompradorId,
            CompradorNombre = t.Comprador is null ? string.Empty : NombreMostrado(t.Comprador),
            VendedorId = t.VendedorId,
            VendedorNombre = t.Vendedor is null ? string.Empty : NombreMostrado(t.Vendedor),
            DivisaOrigen = t.Oferta?.DivisaOrigen ?? string.Empty,
            DivisaDestino = t.Oferta?.DivisaDestino ?? string.Empty,
            MontoOperado = t.MontoOperado,
            TipoCambio = t.TipoCambio,
            Estado = t.Estado,
            FechaInicio = t.FechaInicio,
            FechaLimiteAccion = t.FechaLimiteAccion,
            Historial = t.Historial
                .OrderBy(h => h.Fecha)
                .Select(h => new HistorialEstadoDto { Estado = h.Estado, Comentario = h.Comentario, Fecha = h.Fecha })
                .ToList(),
            Vouchers = t.Vouchers
                .OrderBy(v => v.FechaSubida)
                .Select(AVoucherDto)
                .ToList()
        };
        return dto;
    }

    public static VoucherDto AVoucherDto(Voucher v) => new()
    {
        Id = v.Id,
        Tipo = v.Tipo,
        NombreArchivo = v.NombreArchivo,
        RutaArchivo = v.RutaArchivo,
        NumeroOperacion = v.NumeroOperacion,
        FechaDeposito = v.FechaDeposito,
        FechaSubida = v.FechaSubida
    };

    public static DisputaDto ADisputaDto(Disputa d) => new()
    {
        Id = d.Id,
        TransaccionId = d.TransaccionId,
        TransaccionCodigo = d.Transaccion?.Codigo ?? string.Empty,
        AbiertaPorId = d.AbiertaPorId,
        Motivo = d.Motivo,
        Estado = d.Estado,
        Resolucion = d.Resolucion,
        ComentarioResolucion = d.ComentarioResolucion,
        FechaApertura = d.FechaApertura,
        FechaResolucion = d.FechaResolucion,
        Evidencias = d.Evidencias.Select(e => e.RutaArchivo).ToList()
    };

    public static NotificacionDto ANotificacionDto(Notificacion n) => new()
    {
        Id = n.Id,
        Titulo = n.Titulo,
        Descripcion = n.Descripcion,
        Enlace = n.Enlace,
        Leida = n.Leida,
        Fecha = n.Fecha
    };

    public static CalificacionDto ACalificacionDto(Calificacion c) => new()
    {
        Id = c.Id,
        TransaccionId = c.TransaccionId,
        CalificadorId = c.CalificadorId,
        CalificadoId = c.CalificadoId,
        Estrellas = c.Estrellas,
        Comentario = c.Comentario,
        Fecha = c.Fecha
    };
}
