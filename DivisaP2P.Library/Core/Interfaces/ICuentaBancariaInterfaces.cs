using DivisaP2P.Library.Core.Common;
using DivisaP2P.Library.Core.DTOs;
using DivisaP2P.Library.Core.Entities;

namespace DivisaP2P.Library.Core.Interfaces;

public interface ICuentaBancariaRepository
{
    Task<IEnumerable<CuentaBancaria>> GetByUsuarioAsync(int usuarioId);
    Task<CuentaBancaria?> GetByIdAsync(int id);
    Task<CuentaBancaria> AddAsync(CuentaBancaria cuenta);
    Task UpdateAsync(CuentaBancaria cuenta);
    Task DeleteAsync(CuentaBancaria cuenta);
    /// <summary>Cuentas de la misma divisa marcadas como predeterminadas (para desmarcarlas).</summary>
    Task<IEnumerable<CuentaBancaria>> GetPredeterminadasAsync(int usuarioId, string divisa);
    Task<bool> TieneOfertasActivasAsync(int cuentaId);
}

public interface ICuentaBancariaService
{
    Task<IEnumerable<CuentaBancariaDto>> ListarAsync(int usuarioId);
    Task<ResultadoServicio<CuentaBancariaDto>> CrearAsync(int usuarioId, CuentaBancariaCreateDto dto);
    Task<ResultadoServicio> EliminarAsync(int usuarioId, int cuentaId);
}
