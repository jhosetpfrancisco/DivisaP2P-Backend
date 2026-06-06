using DivisaP2P.Library.Core.Common;
using DivisaP2P.Library.Core.DTOs;
using DivisaP2P.Library.Core.Entities;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.Library.Infrastructure.Services;

/// <summary>
/// Publicación, búsqueda, edición y matching de ofertas (US-004 a US-007, US-020).
/// Aplica las reglas de negocio de montos, vigencia y límite de ofertas activas.
/// </summary>
public class OfertaService : IOfertaService
{
    private readonly IOfertaRepository _ofertas;
    private readonly ICuentaBancariaRepository _cuentas;

    public OfertaService(IOfertaRepository ofertas, ICuentaBancariaRepository cuentas)
    {
        _ofertas = ofertas;
        _cuentas = cuentas;
    }

    public async Task<ResultadoServicio<OfertaDto>> PublicarAsync(int usuarioId, string rol, OfertaCreateDto dto)
    {
        if (dto.DivisaOrigen == dto.DivisaDestino)
            return ResultadoServicio<OfertaDto>.Error(CodigoError.Validacion,
                "La divisa origen y destino deben ser diferentes.");

        if (!Divisas.EsValida(dto.DivisaOrigen) || !Divisas.EsValida(dto.DivisaDestino))
            return ResultadoServicio<OfertaDto>.Error(CodigoError.Validacion, "Divisa no soportada.");

        // Máximo 4 decimales en el tipo de cambio (US-004).
        if (decimal.Round(dto.TipoCambio, 4) != dto.TipoCambio)
            return ResultadoServicio<OfertaDto>.Error(CodigoError.Validacion,
                "El tipo de cambio admite como máximo 4 decimales.");

        // Tope de monto según el rol (RN-005).
        var esEmpresa = rol == Roles.Empresa;
        var montoMaximo = esEmpresa ? ReglasNegocio.MontoMaximoEmpresa : ReglasNegocio.MontoMaximoUsuario;
        if (dto.Monto < ReglasNegocio.MontoMinimoOferta || dto.Monto > montoMaximo)
            return ResultadoServicio<OfertaDto>.Error(CodigoError.Validacion,
                $"El monto debe estar entre {ReglasNegocio.MontoMinimoOferta} y {montoMaximo}.");

        // La cuenta bancaria debe pertenecer al usuario (US-004 / US-003).
        var cuenta = await _cuentas.GetByIdAsync(dto.CuentaBancariaId);
        if (cuenta is null || cuenta.UsuarioId != usuarioId)
            return ResultadoServicio<OfertaDto>.Error(CodigoError.Validacion,
                "La cuenta bancaria seleccionada no existe o no te pertenece.");

        // Límite de 5 ofertas activas para usuarios regulares (RN-007).
        if (!esEmpresa)
        {
            var activas = await _ofertas.ContarActivasPorUsuarioAsync(usuarioId);
            if (activas >= ReglasNegocio.MaxOfertasActivasUsuario)
                return ResultadoServicio<OfertaDto>.Error(CodigoError.Conflicto,
                    $"Alcanzaste el máximo de {ReglasNegocio.MaxOfertasActivasUsuario} ofertas activas.");
        }

        var ahora = DateTime.UtcNow;
        var oferta = new Oferta
        {
            UsuarioId = usuarioId,
            TipoOperacion = dto.TipoOperacion,
            DivisaOrigen = dto.DivisaOrigen,
            DivisaDestino = dto.DivisaDestino,
            MontoTotal = dto.Monto,
            MontoDisponible = dto.Monto,
            TipoCambio = dto.TipoCambio,
            Estado = EstadosOferta.Activa,
            CuentaBancariaId = dto.CuentaBancariaId,
            EsVolumenEtu = esEmpresa,
            FechaPublicacion = ahora,
            FechaExpiracion = ahora.AddHours(ReglasNegocio.VigenciaOfertaHoras)
        };

        var creada = await _ofertas.AddAsync(oferta);
        var conUsuario = await _ofertas.GetByIdAsync(creada.Id);
        return ResultadoServicio<OfertaDto>.Ok(Mapeos.AOfertaDto(conUsuario!), "Oferta publicada.");
    }

    public async Task<ResultadoPaginado<OfertaDto>> BuscarAsync(OfertaFiltroDto filtro, int usuarioId)
    {
        // Expira las ofertas vencidas antes de listar, para no mostrar ofertas caducadas.
        await _ofertas.ExpirarVencidasAsync(DateTime.UtcNow);

        var (items, total) = await _ofertas.BuscarAsync(filtro, usuarioId);
        return new ResultadoPaginado<OfertaDto>
        {
            Data = items.Select(Mapeos.AOfertaDto),
            Total = total,
            Pagina = filtro.Pagina,
            TamanioPagina = filtro.TamanioPagina
        };
    }

    public async Task<OfertaDto?> GetByIdAsync(int id)
    {
        var oferta = await _ofertas.GetByIdAsync(id);
        return oferta is null ? null : Mapeos.AOfertaDto(oferta);
    }

    public async Task<IEnumerable<OfertaDto>> MisOfertasAsync(int usuarioId)
    {
        var ofertas = await _ofertas.GetPorUsuarioAsync(usuarioId);
        return ofertas.Select(Mapeos.AOfertaDto);
    }

    public async Task<ResultadoServicio> ActualizarTipoCambioAsync(int usuarioId, int ofertaId, OfertaUpdateDto dto)
    {
        var oferta = await _ofertas.GetByIdAsync(ofertaId);
        if (oferta is null)
            return ResultadoServicio.Error(CodigoError.NoEncontrado, "Oferta no encontrada.");

        if (oferta.UsuarioId != usuarioId)
            return ResultadoServicio.Error(CodigoError.NoAutorizado, "No puedes editar una oferta que no es tuya.");

        if (oferta.Estado != EstadosOferta.Activa)
            return ResultadoServicio.Error(CodigoError.Validacion, "Solo se pueden editar ofertas activas.");

        // No se permite editar si ya tiene transacciones en curso (US-006).
        if (oferta.MontoDisponible != oferta.MontoTotal)
            return ResultadoServicio.Error(CodigoError.Conflicto,
                "No se puede editar una oferta con transacciones en curso.");

        if (decimal.Round(dto.TipoCambio, 4) != dto.TipoCambio)
            return ResultadoServicio.Error(CodigoError.Validacion, "El tipo de cambio admite como máximo 4 decimales.");

        oferta.TipoCambio = dto.TipoCambio;
        await _ofertas.UpdateAsync(oferta);
        return ResultadoServicio.Ok("Tipo de cambio actualizado.");
    }

    public async Task<ResultadoServicio> CancelarAsync(int usuarioId, int ofertaId)
    {
        var oferta = await _ofertas.GetByIdAsync(ofertaId);
        if (oferta is null)
            return ResultadoServicio.Error(CodigoError.NoEncontrado, "Oferta no encontrada.");

        if (oferta.UsuarioId != usuarioId)
            return ResultadoServicio.Error(CodigoError.NoAutorizado, "No puedes cancelar una oferta que no es tuya.");

        if (oferta.Estado != EstadosOferta.Activa)
            return ResultadoServicio.Error(CodigoError.Validacion, "Solo se pueden cancelar ofertas activas.");

        if (oferta.MontoDisponible != oferta.MontoTotal)
            return ResultadoServicio.Error(CodigoError.Conflicto,
                "No se puede cancelar una oferta con transacciones en curso.");

        oferta.Estado = EstadosOferta.Cancelada;
        await _ofertas.UpdateAsync(oferta);
        return ResultadoServicio.Ok("Oferta cancelada.");
    }

    public async Task<ResultadoServicio<IEnumerable<OfertaDto>>> MatchingAsync(int usuarioId, int ofertaId)
    {
        var oferta = await _ofertas.GetByIdAsync(ofertaId);
        if (oferta is null)
            return ResultadoServicio<IEnumerable<OfertaDto>>.Error(CodigoError.NoEncontrado, "Oferta no encontrada.");

        if (oferta.UsuarioId != usuarioId)
            return ResultadoServicio<IEnumerable<OfertaDto>>.Error(CodigoError.NoAutorizado,
                "Solo puedes ver matches de tus propias ofertas.");

        var candidatas = await _ofertas.GetCandidatasMatchingAsync(oferta);

        // Ordenar por mejor tipo de cambio y mayor calificación del oferente (US-007).
        var ordenadas = candidatas
            .OrderByDescending(o => o.TipoCambio)
            .ThenByDescending(o => o.Usuario.CalificacionPromedio)
            .Select(Mapeos.AOfertaDto)
            .ToList();

        var mensaje = ordenadas.Count == 0 ? "No se encontraron ofertas compatibles." : null;
        return ResultadoServicio<IEnumerable<OfertaDto>>.Ok(ordenadas, mensaje);
    }
}
