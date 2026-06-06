using DivisaP2P.Library.Core.Entities;

namespace DivisaP2P.Library.Core.Interfaces;

/// <summary>Genera tokens JWT para los usuarios autenticados (US-002).</summary>
public interface IJwtService
{
    (string Token, DateTime Expira) GenerarToken(Usuario usuario);
}
