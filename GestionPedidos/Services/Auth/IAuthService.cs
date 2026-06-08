using GestionPedidos.Contracts.Auth;
using GestionPedidos.Models;

namespace GestionPedidos.Services;

public interface IAuthService
{
    /// <summary>
    /// Login para empleados y administradores
    /// </summary>
    Task<AuthResponse> LoginAdminAsync(LoginRequest request);

    /// <summary>
    /// Login para clientes
    /// </summary>
    Task<AuthResponse> LoginClientAsync(LoginRequest request);

    /// <summary>
    /// Registrar nuevo cliente
    /// </summary>
    Task<AuthResponse> RegisterClienteAsync(RegisterClienteRequest request);

    /// <summary>
    /// Cambiar contraseña del usuario actual
    /// </summary>
    Task<AuthResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);

    /// <summary>
    /// Obtener perfil del usuario actual
    /// </summary>
    Task<UserProfileResponse?> GetUserProfileAsync(Guid userId);

    /// <summary>
    /// Obtener información del usuario por ID
    /// </summary>
    Task<etUsuario?> GetUserByIdAsync(Guid userId);

    /// <summary>
    /// Logout (invalidar token - opcional si se usa con JWT stateless)
    /// </summary>
    Task LogoutAsync(Guid userId);
}

