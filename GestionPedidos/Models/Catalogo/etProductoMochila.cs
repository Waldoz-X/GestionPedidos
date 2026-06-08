namespace GestionPedidos.Models;

/// <summary>
/// Especialización CTI: Mochilas, Bolsas, Zapateras. Serie IH.
/// EsUnitalla = true → un solo SKU por variante.
/// </summary>
public class etProductoMochila
{
    public Guid IdProducto { get; set; }
    public string ClSubcategoria { get; set; } = null!;
    public string? DsMaterialPrincipal { get; set; }
    public decimal? NoCapacidadLitros { get; set; }
    public int? NoCompartimentos { get; set; }
    public string? DsDimensiones { get; set; }
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
