namespace GestionPedidos.Models;

/// <summary>
/// SKU = UNIDAD MÍNIMA VENDIBLE (artículo físico real del carrito de compras).
/// Representa: Producto + Combinación de Color (Variante) + Talla
///
/// JERARQUÍA DE NEGOCIO (estilo catálogo Rinat / Amazon):
///   etProducto  → Producto base con su categoría (ARIES PRO → Subcategoría PROFESIONAL de GUANTE)
///   etVariante  → Combinación de color (601 "AZUL/ROSA") + foto
///   etSku       → ★ ARTÍCULO VENDIBLE (variante + talla 5.0) = lo que metes al carrito
///
/// Ejemplo: GPM5I2 (ASIMETRIK PRIME) + 601 (AZUL/ROSA) + 5.0 = 1 SKU
/// Este es el nivel donde se controlan:
///   - Existencias (NoStockDisponible / NoStockReservado)
///   - Código de barras final (ClCodigoBarras / EAN)
///   - Código de item interno (ClItem: "1GPM5I2Y50-601-217")
/// </summary>
public class etSku
{
    public Guid IdSku { get; set; }
    public Guid IdVariante { get; set; }
    public int? IdElemTalla { get; set; }           // FK a CCatalogoElemento (Catálogo: TALLAS/PUNTOS)
    
    public string? ClItem { get; set; }             // Código único del SKU: "1GPM5I2Y50-601-217"
    public string? ClCodigoBarras { get; set; }     // Código de barras EAN
    public string ClEstatusSku { get; set; } = "ACTIVO";
    
    // --- Inventario ---
    public int NoStockDisponible { get; set; } = 0;
    public int NoStockReservado { get; set; } = 0;
    // StockNeto = NoStockDisponible - NoStockReservado

    // Auditoría
    public required string ClOperadorCrea { get; set; }
    public string? ClOperadorModifica { get; set; }
    public required string NbArtefactoCrea { get; set; }
    public string? NbArtefactoModifica { get; set; }
    public DateTimeOffset FeCreacion { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FeModificacion { get; set; }

    // ── Navegación ──
    public Variante Variante { get; set; } = null!;
    public CCatalogoElemento? Talla { get; set; }
    public ICollection<Precio> Precios { get; set; } = [];
    public ICollection<LineaPedido> LineasPedido { get; set; } = [];
}
