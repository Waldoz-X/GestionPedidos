using GestionPedidos.Models.Catalogo;

namespace GestionPedidos.Models.Catalogo;

/// <summary>
/// Tabla de PRODUCTOS. Representa un producto específico dentro de una categoría (División o Subcategoría).
/// 
/// Un mismo modelo puede tener múltiples productos (variaciones por gama/segmento).
/// Ejemplo: ARIES puede tener ARIES PRO, ARIES PRIME, ARIES AERO.
/// Cada uno es un producto independiente con su propia:
/// - Categoría (División raíz o Subcategoría hija, ej: GUANTE → PROFESIONAL)
/// - Variantes (combinaciones de color)
/// - SKUs (variante + talla)
/// - Inventario
/// </summary>
public class etProducto
{
    public Guid IdProducto { get; set; }
    
    public string? ClProducto { get; set; }                  // Clave/Código corto del producto (ej: "ARKJAD-PRO")
    public string NbProducto { get; set; } = null!;          // Nombre comercial completo (GOAL KEEPER JERSEY ARKANO ADU, ARIES PRO, etc.)

    // --- Catálogos Dinámicos (FKs a CCatalogoElemento) ---
    public int IdElemCategoria { get; set; }        // FK a CCatalogoElemento (Categoría: División o Subcategoría)
    public int? IdElemLineaColeccion { get; set; }  // FK a CCatalogoElemento (Catálogo: LINEAS_COLECCION)


    public string? ClHsCode { get; set; }
    
    public string ClEstatusProducto { get; set; } = "ACTIVO"; // Estatus del registro (Activo/Inactivo)

    // Auditoría
    public required string ClOperadorCrea { get; set; }
    public string? ClOperadorModifica { get; set; }
    public required string NbArtefactoCrea { get; set; }
    public string? NbArtefactoModifica { get; set; }
    public DateTimeOffset FeCreacion { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FeModificacion { get; set; }

    // ── Navegación a Catálogos Dinámicos ──
    public CCatalogoElemento Categoria { get; set; } = null!;
    public CCatalogoElemento? LineaColeccion { get; set; }


    public ICollection<etVariante> Variantes { get; set; } = [];
}