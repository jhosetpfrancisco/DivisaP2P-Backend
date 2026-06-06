using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using DivisaP2P.Library.Core.Common;
using DivisaP2P.Library.Core.Entities;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.Library.Infrastructure.Services;

/// <summary>
/// Genera tokens JWT firmados (HS256) con expiración de 60 minutos (RN/US-002).
/// Los claims incluyen el id del usuario y su rol, que la API usa para autorizar.
/// </summary>
public class JwtService : IJwtService
{
    private readonly JwtSettings _settings;

    public JwtService(JwtSettings settings)
    {
        _settings = settings;
    }

    public (string Token, DateTime Expira) GenerarToken(Usuario usuario)
    {
        var expira = DateTime.UtcNow.AddMinutes(_settings.MinutosExpiracion);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Role, usuario.Rol),
            new(JwtRegisteredClaimNames.Email, usuario.Correo),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.ClaveSecreta));
        var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expira,
            signingCredentials: credenciales);

        return (new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}
