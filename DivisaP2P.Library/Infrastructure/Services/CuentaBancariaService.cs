using DivisaP2P.Library.Core.Common;
using DivisaP2P.Library.Core.DTOs;
using DivisaP2P.Library.Core.Entities;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.Library.Infrastructure.Services;

/// <summary>
/// Cuentas bancarias del usuario (US-003): al menos una es obligatoria para
/// publicar ofertas. Se permite una predeterminada por divisa.
/// </summary>
public class CuentaBancariaService : ICuentaBancariaService
{
    private readonly ICuentaBancariaRepository _cuentas;

    public CuentaBancariaService(ICuentaBancariaRepository cuentas)
    {
        _cuentas = cuentas;
    }

    public async Task<IEnumerable<CuentaBancariaDto>> ListarAsync(int usuarioId)
    {
        var cuentas = await _cuentas.GetByUsuarioAsync(usuarioId);
        return cuentas.Select(Mapeos.ACuentaBancariaDto);
    }

    public async Task<ResultadoServicio<CuentaBancariaDto>> CrearAsync(int usuarioId, CuentaBancariaCreateDto dto)
    {
        var cuenta = new CuentaBancaria
        {
            UsuarioId = usuarioId,
            Banco = dto.Banco,
            TipoCuenta = dto.TipoCuenta,
            Divisa = dto.Divisa,
            NumeroCuenta = dto.NumeroCuenta,
            Cci = dto.Cci,
            NombreTitular = dto.NombreTitular,
            EsPredeterminada = dto.EsPredeterminada
        };

        // Solo una cuenta predeterminada por divisa: desmarcar las anteriores.
        if (dto.EsPredeterminada)
        {
            var previas = await _cuentas.GetPredeterminadasAsync(usuarioId, dto.Divisa);
            foreach (var previa in previas)
            {
                previa.EsPredeterminada = false;
                await _cuentas.UpdateAsync(previa);
            }
        }

        var creada = await _cuentas.AddAsync(cuenta);
        return ResultadoServicio<CuentaBancariaDto>.Ok(Mapeos.ACuentaBancariaDto(creada), "Cuenta bancaria registrada.");
    }

    public async Task<ResultadoServicio> EliminarAsync(int usuarioId, int cuentaId)
    {
        var cuenta = await _cuentas.GetByIdAsync(cuentaId);
        if (cuenta is null || cuenta.UsuarioId != usuarioId)
            return ResultadoServicio.Error(CodigoError.NoEncontrado, "Cuenta bancaria no encontrada.");

        if (await _cuentas.TieneOfertasActivasAsync(cuentaId))
            return ResultadoServicio.Error(CodigoError.Conflicto,
                "No puedes eliminar una cuenta asociada a ofertas activas.");

        await _cuentas.DeleteAsync(cuenta);
        return ResultadoServicio.Ok("Cuenta bancaria eliminada.");
    }
}
