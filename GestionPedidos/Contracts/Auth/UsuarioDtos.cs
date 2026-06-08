namespace GestionPedidos.Contracts.Auth;

/// <summary>
/// DTO para registrar un nuevo usuario empleado
/// </summary>
public record RegisterEmpleadoRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    string ClEmpleado,        // Código del empleado
    string NbEmpleado,        // Nombre del empleado
    string NbApellidos,       // Apellidos del empleado
    int? IdElemArea           // ID del área/departamento
);

/// <summary>
/// DTO para registrar un nuevo usuario de cliente
/// </summary>
public record RegisterClienteUserRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    Guid IdCliente            // El cliente ya debe existir
);

/// <summary>
/// DTO para crear un usuario con acceso para empleado (Admin)
/// </summary>
public record CreateEmpleadoUserDto(
    string Email,
    string Password,
    Guid IdEmpleado           // El empleado ya debe existir
);

/// <summary>
/// DTO para crear un usuario con acceso para cliente (Admin)
/// </summary>
public record CreateClienteUserDto(
    string Email,
    string Password,
    Guid IdCliente            // El cliente ya debe existir
);

/// <summary>
/// Respuesta de creación de usuario
/// </summary>
public record CreateUserResponse(
    bool Success,
    string Message,
    CreatedUserData? Data,
    string[]? Errors = null
);

/// <summary>
/// Datos del usuario creado
/// </summary>
public record CreatedUserData(
    Guid IdUsuario,
    string Email,
    string UserName,
    string TipoUsuario,  // EMPLEADO o CLIENTE
    string[] Roles
);

/// <summary>
/// DTO para cambiar el estado de un usuario (ACTIVO | INACTIVO)
/// </summary>
public record CambiarEstadoUsuarioRequest(string NuevoEstado);

/// <summary>
/// DTO para resetear la contraseña de un usuario
/// </summary>
public record ResetPasswordRequest(string NuevaPassword);

/// <summary>
/// DTO para listar todos los usuarios con su información completa
/// </summary>
public record UsuarioListDto(
    Guid IdUsuario,
    string Email,
    string TipoUsuario,           // EMPLEADO | CLIENTE | ADMIN
    string ClEstatusUsuario,
    string[] Roles,
    DateTimeOffset FeCreacion,
    UsuarioEmpleadoDto? Empleado,
    UsuarioClienteDto? Cliente
);

/// <summary>
/// Datos del empleado dentro del listado de usuarios
/// </summary>
public record UsuarioEmpleadoDto(
    Guid IdEmpleado,
    string ClEmpleado,
    string NbEmpleado,
    string NbApellidos,
    string NbNombreCompleto,      // NbEmpleado + NbApellidos
    string? NbDepartamento,       // Nombre del área/departamento del catálogo
    string ClEstatusEmpleado
);

/// <summary>
/// Datos del cliente dentro del listado de usuarios
/// </summary>
public record UsuarioClienteDto(
    Guid IdCliente,
    string NbComercial,
    string ClTipoCliente,
    decimal MnLimiteCredito,
    string NbMoneda,              // Nombre de la moneda del catálogo
    string ClEstatusCliente
);

