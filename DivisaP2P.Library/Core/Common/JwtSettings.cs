namespace DivisaP2P.Library.Core.Common;

/// <summary>
/// Configuración del esquema JWT, poblada desde la sección "Jwt" de appsettings.json.
/// </summary>
public class JwtSettings
{
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string ClaveSecreta { get; set; } = null!;
    public int MinutosExpiracion { get; set; } = 60;
}
