namespace GestionPedidos.Contracts.Empleados;

public record EmpleadoCreateDto(
    Guid? IdUsuario,
    int? IdElemArea,
    string? NuEmpleado,
    string ClEmpleado,
    string NbEmpleado,
    string NbApellidos,
    string ClEstatusEmpleado
);

public record EmpleadoUpdateDto(
    Guid? IdUsuario,
    int? IdElemArea,
    string? NuEmpleado,
    string ClEmpleado,
    string NbEmpleado,
    string NbApellidos,
    string ClEstatusEmpleado
);

public record EmpleadoDto(
    Guid IdEmpleado,
    Guid? IdUsuario,
    int? IdElemArea,
    string? NuEmpleado,
    string ClEmpleado,
    string NbEmpleado,
    string NbApellidos,
    string ClEstatusEmpleado,
    string? Correo,
    DateTimeOffset FeCreacion,
    DateTimeOffset? FeModificacion
);
