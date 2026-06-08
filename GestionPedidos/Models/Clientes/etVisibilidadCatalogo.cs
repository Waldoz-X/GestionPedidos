namespace GestionPedidos.Models;

/// <summary>
/// ACL del catálogo. PK compuesta (IdCliente, IdProducto).
/// VISIBLE = acceso normal, OCULTO = cliente no ve, EXCLUSIVO = solo ese cliente.
/// </summary>
public class etVisibilidadCatalogo
{
    public Guid IdCliente { get; set; }
    public Guid IdProducto { get; set; }
    public string ClTipoAcceso { get; set; } = null!;
    public string ClEstatusVisibilidad { get; set; } = "ACTIVO";

    // Auditoría
    public required string ClOperadorCrea { get; set; }
    public string? ClOperadorModifica { get; set; }
    public required string NbArtefactoCrea { get; set; }
    public string? NbArtefactoModifica { get; set; }
    public DateTimeOffset FeCreacion { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FeModificacion { get; set; }

    // ── Navegación ──
    public Cliente Cliente { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
}
