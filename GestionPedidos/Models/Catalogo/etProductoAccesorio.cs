namespace GestionPedidos.Models;

/// <summary>
/// Especialización CTI: Accesorios varios. Gorras, Cinturones, Medias, etc.
/// Pueden llevar talla, punto, ambos o ser unitalla.
/// </summary>
public class etProductoAccesorio
{
    public Guid IdProducto { get; set; }
    public string ClSubcategoria { get; set; } = null!;
    public string? DsMaterialPrincipal { get; set; }
    // ✅ ClTipoMedida movido a etProducto (base)

    // ── Navegación ──
    public Producto Producto { get; set; } = null!;
}
