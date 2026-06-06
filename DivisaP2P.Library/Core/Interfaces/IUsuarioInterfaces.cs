using DivisaP2P.Library.Core.Common;
using DivisaP2P.Library.Core.DTOs;
using DivisaP2P.Library.Core.Entities;

namespace DivisaP2P.Library.Core.Interfaces;

/// <summary>Acceso a datos de Usuario (patrón Repository).</summary>
public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(int id);
    Task<Usuario?> GetByCorreoAsync(string correo);
    Task<bool> ExisteCorreoAsync(string correo);
    Task<bool> ExisteRucAsync(string ruc);
    Task<Usuario> AddAsync(Usuario usuario);
    Task UpdateAsync(Usuario usuario);
    Task<(IEnumerable<Usuario> Items, int Total)> BuscarAsync(
        string? rol, string? estado, decimal? calificacionMin, int pagina, int tamanioPagina);
}

/// <summary>Autenticación: registro de usuarios/empresas e inicio de sesión.</summary>
public interface IAuthService
{
    Task<ResultadoServicio<UsuarioDto>> RegistrarUsuarioAsync(RegistroUsuarioDto dto);
    Task<ResultadoServicio<UsuarioDto>> RegistrarEmpresaAsync(RegistroEmpresaDto dto);
    Task<ResultadoServicio<AuthResponseDto>> LoginAsync(LoginDto dto);

    /// <summary>Simula el clic en el enlace de verificación de correo (US-001).</summary>
    Task<ResultadoServicio> VerificarCorreoAsync(int usuarioId);
}

/// <summary>Gestión de perfil (US-003) y administración de usuarios (US-017, US-019).</summary>
public interface IUsuarioService
{
    Task<UsuarioDto?> GetPerfilAsync(int usuarioId);
    Task<ResultadoServicio> ActualizarPerfilAsync(int usuarioId, PerfilUpdateDto dto);
    Task<ResultadoServicio> CambiarPasswordAsync(int usuarioId, CambioPasswordDto dto);

    Task<ResultadoPaginado<UsuarioDto>> ListarAsync(
        string? rol, string? estado, decimal? calificacionMin, int pagina, int tamanioPagina);
    Task<ResultadoServicio> BloquearAsync(int usuarioId, BloqueoUsuarioDto dto);
    Task<ResultadoServicio> DesbloquearAsync(int usuarioId);
    Task<ResultadoServicio> AprobarEmpresaAsync(int usuarioId);
}
