namespace GestionPedidos.Models;

/// <summary>
/// Línea de detalle del pedido. Precio congelado al confirmar.
/// Subtotal = Cantidad * PrecioUnitario - DescuentoLinea.
/// </summary>
public class etLineaPedido
{
    public Guid IdLineaPedido { get; set; }
    public Guid IdPedido { get; set; }
    public Guid IdSku { get; set; }
    public int NoCantidad { get; set; }
    public decimal MnPrecioUnitario { get; set; }
    public decimal MnDescuentoLinea { get; set; }
    public decimal MnSubtotal { get; set; }

    // Auditoría
    public required string ClOperadorCrea { get; set; }
    public string? ClOperadorModifica { get; set; }
    public required string NbArtefactoCrea { get; set; }
    public string? NbArtefactoModifica { get; set; }
    public DateTimeOffset FeCreacion { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FeModificacion { get; set; }

    // ── Navegación ──
    public Pedido Pedido { get; set; } = null!;
    public Sku Sku { get; set; } = null!;
}
