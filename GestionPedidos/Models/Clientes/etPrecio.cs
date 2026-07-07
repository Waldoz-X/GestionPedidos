namespace GestionPedidos.Models;

/// <summary>
/// Motor de precios. IdCliente NULL → precio lista general.
/// Incluye vigencia temporal para resolución de conflictos.
/// </summary>
public class etPrecio
{
    public Guid IdPrecio { get; set; }
    public Guid IdSku { get; set; }

    /// <summary>
    /// FK a etPoliticaPrecio. NULL = precio exclusivo directo del cliente (sin política).
    /// Si hay IdCliente e IdPolitica NULL = precio especial 1:1 con el cliente.
    /// Si IdCliente es NULL e IdPolitica está seteado = precio por política (aplica a todos los clientes de esa política).
    /// </summary>
    public Guid? IdPolitica { get; set; }
    public Guid? IdCliente { get; set; }
    public decimal MnPrecioNeto { get; set; }
    /// <summary>Clave de moneda: "MXN", "USD", "EUR", "CAD", "GBP"</summary>
    public string ClMoneda { get; set; } = "MXN";
    public string ClEstatusPrecio { get; set; } = "ACTIVO";
    public DateTimeOffset FeVigenteDesde { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FeVigenteHasta { get; set; }

    // Auditoría
    public required string ClOperadorCrea { get; set; }
    public string? ClOperadorModifica { get; set; }
    public required string NbArtefactoCrea { get; set; }
    public string? NbArtefactoModifica { get; set; }
    public DateTimeOffset FeCreacion { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FeModificacion { get; set; }

    // ── Navegación ──
    public Sku Sku { get; set; } = null!;
    public PoliticaPrecio? Politica { get; set; }   // null si es precio directo del cliente
    public Cliente? Cliente { get; set; }
    public ICollection<HistorialPrecio> Historial { get; set; } = [];
}
