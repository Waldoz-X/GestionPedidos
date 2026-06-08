namespace GestionPedidos.Models;

/// <summary>
/// Especialización CTI: Conos de entrenamiento. Serie IK.
/// Generalmente unitalla — el tamaño va en AlturaCm.
/// </summary>
public class etProductoCono
{
    public Guid IdProducto { get; set; }
    public decimal NoAlturaCm { get; set; }
    public string? DsMaterial { get; set; }
    public bool FgEsUnitalla { get; set; }

    // Auditoría
    public required string ClOperadorCrea { get; set; }
    public string? ClOperadorModifica { get; set; }
    public required string NbArtefactoCrea { get; set; }
    public string? NbArtefactoModifica { get; set; }
    public DateTimeOffset FeCreacion { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FeModificacion { get; set; }

    // ── Navegación ──
    public Producto Producto { get; set; } = null!;
}
