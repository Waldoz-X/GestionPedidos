namespace GestionPedidos.Models;

/// <summary>
/// Dirección de envío/facturación del cliente. Un cliente puede tener varias.
/// NUEVA tabla v4.1 — resuelve la falta de dirección de envío.
/// </summary>
public class etDireccionCliente
{
    public Guid IdDireccion { get; set; }
    public Guid IdCliente { get; set; }
    public int IdPais { get; set; }
    public int? IdEstadoCatalogo { get; set; }
    public string? NbAlias { get; set; }
    public string DsLinea1 { get; set; } = null!;
    public string? DsLinea2 { get; set; }
    public string NbCiudad { get; set; } = null!;
    public string? NbEstado { get; set; }
    public string? ClCodigoPostal { get; set; }
    public string ClPais { get; set; } = "MX";
    public string ClEstatusDireccion { get; set; } = "ACTIVO";

    // Auditoría
    public required string ClOperadorCrea { get; set; }
    public string? ClOperadorModifica { get; set; }
    public required string NbArtefactoCrea { get; set; }
    public string? NbArtefactoModifica { get; set; }
    public DateTimeOffset FeCreacion { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FeModificacion { get; set; }

    // ── Navegación ──
    public Cliente Cliente { get; set; } = null!;
    public CatalogoPais PaisCatalogo { get; set; } = null!;
    public CatalogoEstadoPais? EstadoCatalogo { get; set; }
    public ICollection<Pedido> Pedidos { get; set; } = [];
}
