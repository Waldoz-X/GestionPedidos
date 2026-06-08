using GestionPedidos.Models.Catalogo;

namespace GestionPedidos.Models.Catalogo;

/// <summary>
/// Tabla de PRODUCTOS/GAMAS. Representa una línea específica de un código de estilo.
/// 
/// Un mismo código de estilo (ClEstilo en etEstilo) puede tener múltiples Gamas/Productos.
/// Ejemplo: GPM5A3 (ARIES) puede tener ARIES PRO, ARIES PRIME, ARIES AERO.
/// Cada uno es un producto independiente con su propia:
/// - Gama (Prime/Pro/Aero = define palma, composición, características)
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
    public int IdElemDivision { get; set; }         // FK a CCatalogoElemento (Catálogo: DIVISIONES)
    public int? IdElemLineaColeccion { get; set; }  // FK a CCatalogoElemento (Catálogo: LINEAS_COLECCION)
    public int? IdElemGama { get; set; }            // FK a CCatalogoElemento (Catálogo: GAMAS) - define características (Palma, etc)


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
    public CCatalogoElemento Division { get; set; } = null!;
    public CCatalogoElemento? LineaColeccion { get; set; }
    public CCatalogoElemento? Gama { get; set; }


    public ICollection<etVariante> Variantes { get; set; } = [];
}