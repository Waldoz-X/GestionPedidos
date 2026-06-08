namespace GestionPedidos.Models;

/// <summary>
/// Asignacion de cartera comercial entre empleado y cliente.
/// Permite clientes compartidos entre varios empleados.
/// </summary>
public class etAsignacionClienteEmpleado
{
    public Guid IdEmpleado { get; set; }
    public Guid IdCliente { get; set; }
    public string ClTipoRelacion { get; set; } = null!;
    public string ClEstatusAsignacion { get; set; } = "ACTIVO";

    // Auditoría
    public required string ClOperadorCrea { get; set; }
    public string? ClOperadorModifica { get; set; }
    public required string NbArtefactoCrea { get; set; }
    public string? NbArtefactoModifica { get; set; }
    public DateTimeOffset FeCreacion { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FeModificacion { get; set; }

    // -- Navegación --
    public Empleado Empleado { get; set; } = null!;
    public Cliente Cliente { get; set; } = null!;
}
