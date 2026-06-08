namespace GestionPedidos.Models;

/// <summary>
/// Especialización CTI: Espinilleras. Serie IY.
/// Pueden ser unitalla o con talla (S/M/L).
/// </summary>
public class etProductoEspinillera
{
    public Guid IdProducto { get; set; }
    public string? DsMaterial { get; set; }
    public string? DsProteccion { get; set; }
    // ✅ ClTipoMedida movido a etProducto (base)

    // ── Navegación ──
    public Producto Producto { get; set; } = null!;
}
