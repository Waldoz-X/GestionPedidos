namespace GestionPedidos.Contracts.Auth;

/// <summary>
/// DTO para resultado de login exitoso (alias para LoginResponse)
/// </summary>
public record AuthResponse(
    bool Success,
    string Message,
    LoginResponse? Data = null,
    string[]? Errors = null
);

/// <summary>
/// DTO para registro de nuevo cliente
/// </summary>
public record RegisterClienteRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    string NbComercial,
    string ClTipoCliente,          // DISTRIBUIDOR, DIRECTO, ONLINE
    string ClMonedaIdCatalogo      // ID del catálogo de moneda
);

/// <summary>
/// DTO para cambiar contraseña
/// </summary>
public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
);

/// <summary>
/// DTO para perfil de usuario
/// </summary>
public record UserProfileResponse(
    Guid IdUsuario,
    string Email,
    string UserName,
    string TipoUsuario,
    EmpleadoProfileDto? Empleado,
    ClienteProfileDto? Cliente,
    string[] Roles
);

/// <summary>
/// DTO para datos del empleado en perfil
/// </summary>
public record EmpleadoProfileDto(
    Guid IdEmpleado,
    string ClEmpleado,
    string NbEmpleado,
    string NbApellidos,
    string Area
);

/// <summary>
/// DTO para datos del cliente en perfil
/// </summary>
public record ClienteProfileDto(
    Guid IdCliente,
    string NbComercial,
    string ClTipoCliente,
    decimal MnLimiteCredito,
    string Moneda
);


