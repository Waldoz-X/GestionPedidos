using GestionPedidos.Models.Catalogo;

namespace GestionPedidos.Models;

/// <summary>
/// COMBINACIÓN DE COLOR / DISEÑO VISUAL de un producto.
/// Representa el estilo visual (color, diseño gráfico) de un producto específico.
/// 
/// JERARQUÍA DE NEGOCIO (estilo catálogo Rinat / Amazon):
///   etEstilo    → Código base del modelo (GPM5A3 = "ARIES")
///   etProducto  → Gama/especificaciones técnicas (ARIES PRO, palma, composición)
///   etVariante  → ★ COMBINACIÓN DE COLOR (343 "NEGRO/BLANCO/ROJO") + foto del producto en ese color
///   etSku       → Artículo vendible real (combinación + talla = lo que se pone en el carrito)
///
/// Ejemplo: "ARIES PRO" + "343 NEGRO/BLANCO/ROJO" = una variante (esta tabla)
/// El inventario (stock) y código de barras se controlan en etSku (Variante + Talla).
/// Cada variante puede tener múltiples tallas (SKUs) disponibles.
/// Las corridas ya NO se definen como texto rígido aquí; se controlan
/// encendiendo/apagando los SKUs individuales (ClEstatusSku en etSku).
/// </summary>
public class etVariante
{
    public Guid IdVariante { get; set; }
    public Guid IdProducto { get; set; }
    
    // --- Combinación de colores (FK a Catálogo COMBINACIONES) ---
    public int? IdElemCombinacion { get; set; }    // FK → CCatalogoElemento (Catálogo: COMBINACIONES, ej: 343 = NEGRO/BLANCO/ROJO)
    
    public string? UrlImagen { get; set; }          // Imagen de referencia del producto en este color
    public string ClEstatusVariante { get; set; } = "ACTIVO";

    // Auditoría
    public required string ClOperadorCrea { get; set; }
    public string? ClOperadorModifica { get; set; }
    public required string NbArtefactoCrea { get; set; }
    public string? NbArtefactoModifica { get; set; }
    public DateTimeOffset FeCreacion { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FeModificacion { get; set; }

    // ── Navegación ──
    public Producto Producto { get; set; } = null!;
    public CCatalogoElemento? Combinacion { get; set; }
    public ICollection<Sku> Skus { get; set; } = [];
}
