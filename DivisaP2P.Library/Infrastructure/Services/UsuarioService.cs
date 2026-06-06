using DivisaP2P.Library.Core.Common;
using DivisaP2P.Library.Core.DTOs;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.Library.Infrastructure.Services;

/// <summary>
/// Gestión de perfil del propio usuario (US-003) y administración de cuentas
/// por parte del administrador (US-017), más la aprobación de empresas (US-019).
/// </summary>
public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly INotificacionService _notificaciones;

    public UsuarioService(IUsuarioRepository usuarios, INotificacionService notificaciones)
    {
        _usuarios = usuarios;
        _notificaciones = notificaciones;
    }

    public async Task<UsuarioDto?> GetPerfilAsync(int usuarioId)
    {
        var usuario = await _usuarios.GetByIdAsync(usuarioId);
        return usuario is null ? null : Mapeos.AUsuarioDto(usuario);
    }

    public async Task<ResultadoServicio> ActualizarPerfilAsync(int usuarioId, PerfilUpdateDto dto)
    {
        var usuario = await _usuarios.GetByIdAsync(usuarioId);
        if (usuario is null)
            return ResultadoServicio.Error(CodigoError.NoEncontrado, "Usuario no encontrado.");

        usuario.Nombres = dto.Nombres;
        usuario.ApellidoPaterno = dto.ApellidoPaterno;
        usuario.ApellidoMaterno = dto.ApellidoMaterno;
        usuario.Celular = dto.Celular;

        await _usuarios.UpdateAsync(usuario);
        return ResultadoServicio.Ok("Perfil actualizado.");
    }

    public async Task<ResultadoServicio> CambiarPasswordAsync(int usuarioId, CambioPasswordDto dto)
    {
        var usuario = await _usuarios.GetByIdAsync(usuarioId);
        if (usuario is null)
            return ResultadoServicio.Error(CodigoError.NoEncontrado, "Usuario no encontrado.");

        if (!BCrypt.Net.BCrypt.Verify(dto.PasswordActual, usuario.PasswordHash))
            return ResultadoServicio.Error(CodigoError.Validacion, "La contraseña actual no es correcta.");

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordNueva);
        await _usuarios.UpdateAsync(usuario);
        return ResultadoServicio.Ok("Contraseña actualizada.");
    }

    public async Task<ResultadoPaginado<UsuarioDto>> ListarAsync(
        string? rol, string? estado, decimal? calificacionMin, int pagina, int tamanioPagina)
    {
        var (items, total) = await _usuarios.BuscarAsync(rol, estado, calificacionMin, pagina, tamanioPagina);
        return new ResultadoPaginado<UsuarioDto>
        {
            Data = items.Select(Mapeos.AUsuarioDto),
            Total = total,
            Pagina = pagina,
            TamanioPagina = tamanioPagina
        };
    }

    public async Task<ResultadoServicio> BloquearAsync(int usuarioId, BloqueoUsuarioDto dto)
    {
        var usuario = await _usuarios.GetByIdAsync(usuarioId);
        if (usuario is null)
            return ResultadoServicio.Error(CodigoError.NoEncontrado, "Usuario no encontrado.");

        if (usuario.Rol == Roles.Administrador)
            return ResultadoServicio.Error(CodigoError.Validacion, "No se puede bloquear a un administrador.");

        usuario.Estado = EstadosUsuario.Bloqueado;
        usuario.MotivoBloqueo = dto.Motivo;
        await _usuarios.UpdateAsync(usuario);

        await _notificaciones.CrearAsync(usuario.Id, "Cuenta bloqueada",
            "Tu cuenta ha sido bloqueada por el administrador. Motivo: " + dto.Motivo);

        return ResultadoServicio.Ok("Usuario bloqueado.");
    }

    public async Task<ResultadoServicio> DesbloquearAsync(int usuarioId)
    {
        var usuario = await _usuarios.GetByIdAsync(usuarioId);
        if (usuario is null)
            return ResultadoServicio.Error(CodigoError.NoEncontrado, "Usuario no encontrado.");

        usuario.Estado = EstadosUsuario.Activo;
        usuario.MotivoBloqueo = null;
        await _usuarios.UpdateAsync(usuario);

        await _notificaciones.CrearAsync(usuario.Id, "Cuenta reactivada",
            "Tu cuenta ha sido reactivada. Ya puedes operar normalmente.");

        return ResultadoServicio.Ok("Usuario desbloqueado.");
    }

    public async Task<ResultadoServicio> AprobarEmpresaAsync(int usuarioId)
    {
        var usuario = await _usuarios.GetByIdAsync(usuarioId);
        if (usuario is null)
            return ResultadoServicio.Error(CodigoError.NoEncontrado, "Usuario no encontrado.");

        if (usuario.Rol != Roles.Empresa)
            return ResultadoServicio.Error(CodigoError.Validacion, "El usuario no es una empresa de turismo.");

        if (usuario.Estado != EstadosUsuario.PendienteAprobacion)
            return ResultadoServicio.Error(CodigoError.Validacion, "La empresa no está pendiente de aprobación.");

        usuario.Estado = EstadosUsuario.Activo;
        usuario.CorreoVerificado = true;
        await _usuarios.UpdateAsync(usuario);

        await _notificaciones.CrearAsync(usuario.Id, "Empresa aprobada",
            "Tu empresa fue aprobada. Ya puedes iniciar sesión y publicar ofertas.");

        return ResultadoServicio.Ok("Empresa aprobada.");
    }
}
