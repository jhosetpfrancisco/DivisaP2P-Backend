using DivisaP2P.Library.Core.Common;
using DivisaP2P.Library.Core.DTOs;
using DivisaP2P.Library.Core.Entities;
using DivisaP2P.Library.Core.Interfaces;

namespace DivisaP2P.Library.Infrastructure.Services;

/// <summary>
/// Registro de usuarios/empresas e inicio de sesión (US-001, US-002, US-019).
/// Hashea contraseñas con BCrypt y delega la emisión del token a IJwtService.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IJwtService _jwt;

    public AuthService(IUsuarioRepository usuarios, IJwtService jwt)
    {
        _usuarios = usuarios;
        _jwt = jwt;
    }

    public async Task<ResultadoServicio<UsuarioDto>> RegistrarUsuarioAsync(RegistroUsuarioDto dto)
    {
        if (await _usuarios.ExisteCorreoAsync(dto.Correo))
            return ResultadoServicio<UsuarioDto>.Error(CodigoError.Conflicto, "El correo ya está registrado.");

        // Validación de longitud de documento según el tipo (US-001).
        var longitudEsperada = dto.TipoDocumento == "DNI" ? 8 : 9;
        if (dto.NumeroDocumento.Length != longitudEsperada || !dto.NumeroDocumento.All(char.IsDigit))
            return ResultadoServicio<UsuarioDto>.Error(CodigoError.Validacion,
                $"El número de documento para {dto.TipoDocumento} debe tener {longitudEsperada} dígitos.");

        var usuario = new Usuario
        {
            Rol = Roles.Usuario,
            Nombres = dto.Nombres,
            ApellidoPaterno = dto.ApellidoPaterno,
            ApellidoMaterno = dto.ApellidoMaterno,
            Correo = dto.Correo,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            TipoDocumento = dto.TipoDocumento,
            NumeroDocumento = dto.NumeroDocumento,
            Celular = dto.Celular,
            Estado = EstadosUsuario.PendienteVerificacion,
            CorreoVerificado = false,
            CalificacionPromedio = 0m,
            OperacionesCompletadas = 0,
            FechaRegistro = DateTime.UtcNow
        };

        var creado = await _usuarios.AddAsync(usuario);
        return ResultadoServicio<UsuarioDto>.Ok(Mapeos.AUsuarioDto(creado),
            "Registro exitoso. Revisa tu correo para verificar la cuenta.");
    }

    public async Task<ResultadoServicio<UsuarioDto>> RegistrarEmpresaAsync(RegistroEmpresaDto dto)
    {
        if (await _usuarios.ExisteCorreoAsync(dto.Correo))
            return ResultadoServicio<UsuarioDto>.Error(CodigoError.Conflicto, "El correo ya está registrado.");

        if (await _usuarios.ExisteRucAsync(dto.Ruc))
            return ResultadoServicio<UsuarioDto>.Error(CodigoError.Conflicto, "El RUC ya está registrado.");

        var empresa = new Usuario
        {
            Rol = Roles.Empresa,
            Nombres = dto.RazonSocial,
            RazonSocial = dto.RazonSocial,
            Ruc = dto.Ruc,
            RepresentanteLegal = dto.RepresentanteLegal,
            Correo = dto.Correo,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Celular = dto.Celular,
            // La ETU queda pendiente de aprobación del administrador (US-019).
            Estado = EstadosUsuario.PendienteAprobacion,
            CorreoVerificado = false,
            FechaRegistro = DateTime.UtcNow
        };

        var creada = await _usuarios.AddAsync(empresa);
        return ResultadoServicio<UsuarioDto>.Ok(Mapeos.AUsuarioDto(creada),
            "Registro recibido. Una vez aprobado por el administrador podrás iniciar sesión.");
    }

    public async Task<ResultadoServicio<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var usuario = await _usuarios.GetByCorreoAsync(dto.Correo);
        if (usuario is null)
            return ResultadoServicio<AuthResponseDto>.Error(CodigoError.NoAutorizado, "Correo o contraseña incorrectos.");

        // Cuenta bloqueada temporalmente por intentos fallidos (US-002).
        if (usuario.BloqueadoHasta.HasValue && usuario.BloqueadoHasta.Value > DateTime.UtcNow)
        {
            var minutos = (int)Math.Ceiling((usuario.BloqueadoHasta.Value - DateTime.UtcNow).TotalMinutes);
            return ResultadoServicio<AuthResponseDto>.Error(CodigoError.NoAutorizado,
                $"Cuenta bloqueada temporalmente. Intenta nuevamente en {minutos} minuto(s).");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
        {
            usuario.IntentosFallidos++;
            if (usuario.IntentosFallidos >= ReglasNegocio.MaxIntentosLogin)
            {
                usuario.BloqueadoHasta = DateTime.UtcNow.AddMinutes(ReglasNegocio.MinutosBloqueoLogin);
                usuario.IntentosFallidos = 0;
            }
            await _usuarios.UpdateAsync(usuario);
            return ResultadoServicio<AuthResponseDto>.Error(CodigoError.NoAutorizado, "Correo o contraseña incorrectos.");
        }

        if (usuario.Estado == EstadosUsuario.Bloqueado)
            return ResultadoServicio<AuthResponseDto>.Error(CodigoError.NoAutorizado,
                "Tu cuenta está bloqueada. Contacta al administrador.");

        if (usuario.Estado == EstadosUsuario.PendienteAprobacion)
            return ResultadoServicio<AuthResponseDto>.Error(CodigoError.NoAutorizado,
                "Tu empresa aún está pendiente de aprobación por el administrador.");

        // El correo debe estar verificado para acceder (US-002). Los ADM se siembran verificados.
        if (!usuario.CorreoVerificado && usuario.Rol != Roles.Administrador)
            return ResultadoServicio<AuthResponseDto>.Error(CodigoError.NoAutorizado,
                "Debes verificar tu correo antes de iniciar sesión.");

        // Reset de intentos al autenticar correctamente.
        if (usuario.IntentosFallidos > 0 || usuario.BloqueadoHasta.HasValue)
        {
            usuario.IntentosFallidos = 0;
            usuario.BloqueadoHasta = null;
            await _usuarios.UpdateAsync(usuario);
        }

        var (token, expira) = _jwt.GenerarToken(usuario);
        var respuesta = new AuthResponseDto
        {
            Token = token,
            ExpiraEn = expira,
            UsuarioId = usuario.Id,
            Rol = usuario.Rol,
            NombreMostrado = usuario.RazonSocial ?? usuario.Nombres,
            Correo = usuario.Correo
        };

        return ResultadoServicio<AuthResponseDto>.Ok(respuesta);
    }

    public async Task<ResultadoServicio> VerificarCorreoAsync(int usuarioId)
    {
        var usuario = await _usuarios.GetByIdAsync(usuarioId);
        if (usuario is null)
            return ResultadoServicio.Error(CodigoError.NoEncontrado, "Usuario no encontrado.");

        usuario.CorreoVerificado = true;
        if (usuario.Estado == EstadosUsuario.PendienteVerificacion)
            usuario.Estado = EstadosUsuario.Activo;

        await _usuarios.UpdateAsync(usuario);
        return ResultadoServicio.Ok("Correo verificado. Ya puedes iniciar sesión.");
    }
}
