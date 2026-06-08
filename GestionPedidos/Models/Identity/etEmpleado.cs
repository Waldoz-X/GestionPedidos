namespace GestionPedidos.Models;

/// <summary>
/// Perfil interno para usuarios empleados que administran la operacion.
/// Se vincula 1:1 con AspNetUsers.
/// </summary>
public class etEmpleado
{
    public Guid IdEmpleado { get; set; }
    public Guid? IdUsuario { get; set; }
    public int? IdElemArea { get; set; }  // FK a CCatalogoElemento (Catálogo: AREAS_DEPARTAMENTOS)
    public string? NuEmpleado { get; set; } // Número de empleado
    public string ClEmpleado { get; set; } = null!;
    public string NbEmpleado { get; set; } = null!;
    public string NbApellidos { get; set; } = null!;
    public string ClEstatusEmpleado { get; set; } = "ACTIVO";

    // Auditor�a
    public required string ClOperadorCrea { get; set; }
    public string? ClOperadorModifica { get; set; }
    public required string NbArtefactoCrea { get; set; }
    public string? NbArtefactoModifica { get; set; }
    public DateTimeOffset FeCreacion { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FeModificacion { get; set; }

    // -- Navegación --
    public Usuario? Usuario { get; set; }
    public CCatalogoElemento? Area { get; set; }  // Navegación a Catálogo de Áreas/Departamentos
    public ICollection<AsignacionClienteEmpleado> AsignacionesCliente { get; set; } = [];
}
