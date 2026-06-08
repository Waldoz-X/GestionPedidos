namespace GestionPedidos.Contracts.Empleados;

/// <summary>
/// DTO para crear una asignación de cliente a empleado
/// </summary>
public record AsignacionClienteEmpleadoCreateDto(
    Guid IdEmpleado,
    Guid IdCliente,
    string ClTipoRelacion  // PRINCIPAL, RESPALDO, TEMPORAL, etc.
);

/// <summary>
/// DTO para actualizar una asignación de cliente a empleado
/// </summary>
public record AsignacionClienteEmpleadoUpdateDto(
    string ClTipoRelacion,
    string ClEstatusAsignacion  // ACTIVO, INACTIVO, PAUSADO
);

/// <summary>
/// DTO para obtener datos de una asignación
/// </summary>
public record AsignacionClienteEmpleadoDto(
    Guid IdEmpleado,
    Guid IdCliente,
    string ClTipoRelacion,
    string ClEstatusAsignacion,
    DateTimeOffset FeCreacion,
    DateTimeOffset? FeModificacion
);

/// <summary>
/// DTO para obtener asignación con datos del empleado y cliente
/// </summary>
public record AsignacionClienteEmpleadoDetalleDto(
    Guid IdEmpleado,
    string ClEmpleado,
    string? NuEmpleado,
    string NbEmpleado,
    string NbApellidos,
    Guid IdCliente,
    string NbComercial,
    string ClTipoCliente,
    string ClTipoRelacion,
    string ClEstatusAsignacion,
    DateTimeOffset FeCreacion
);

/// <summary>
/// DTO para obtener clientes asignados a un empleado
/// </summary>
public record ClientesDelEmpleadoDto(
    Guid IdCliente,
    string NbComercial,
    string ClTipoCliente,
    string ClTipoRelacion,
    string ClEstatusAsignacion
);

/// <summary>
/// DTO para obtener empleados asignados a un cliente
/// </summary>
public record EmpleadosDelClienteDto(
    Guid IdEmpleado,
    string ClEmpleado,
    string? NuEmpleado,
    string NbEmpleado,
    string NbApellidos,
    string ClTipoRelacion,
    string ClEstatusAsignacion
);

