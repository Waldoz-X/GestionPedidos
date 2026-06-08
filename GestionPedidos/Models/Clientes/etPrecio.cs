namespace GestionPedidos.Models;

/// <summary>
/// Motor de precios. IdCliente NULL → precio lista general.
/// Incluye vigencia temporal para resolución de conflictos.
/// </summary>
public class etPrecio
{
    public Guid IdPrecio { get; set; }
    public Guid IdSku { get; set; }
    public Guid IdPolitica { get; set; }
    public Guid? IdCliente { get; set; }
    public decimal MnPrecioNeto { get; set; }
    public Moneda ClMoneda { get; set; }
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
    public PoliticaPrecio Politica { get; set; } = null!;
    public Cliente? Cliente { get; set; }
    public ICollection<HistorialPrecio> Historial { get; set; } = [];
}
