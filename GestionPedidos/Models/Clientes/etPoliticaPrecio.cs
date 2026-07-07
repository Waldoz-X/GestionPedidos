namespace GestionPedidos.Models;

/// <summary>
/// Política de precios: BASE_SEGMENTO | CLIENTE_ESPECIFICO | DESCUENTO_VOLUMEN.
/// Incluye vigencia temporal (vigente_desde, vigente_hasta).
/// </summary>
public class etPoliticaPrecio
{
    public Guid IdPolitica { get; set; }
    public string NbPolitica { get; set; } = null!;
    public string ClTipoPolitica { get; set; } = null!;
    public int NoPrioridad { get; set; }
    public decimal MnFactorDescuento { get; set; }
    public DateTimeOffset FeVigenteDesde { get; set; }
    public DateTimeOffset? FeVigenteHasta { get; set; }
    public string ClEstatusPolitica { get; set; } = "ACTIVO";

    // Auditoría
    public required string ClOperadorCrea { get; set; }
    public string? ClOperadorModifica { get; set; }
    public required string NbArtefactoCrea { get; set; }
    public string? NbArtefactoModifica { get; set; }
    public DateTimeOffset FeCreacion { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FeModificacion { get; set; }

    // ── Navegación ──
    public ICollection<etPrecio> Precios { get; set; } = [];
    public ICollection<etClientePolitica> Clientes { get; set; } = [];
}
