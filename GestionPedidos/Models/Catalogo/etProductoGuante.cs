namespace GestionPedidos.Models;

/// <summary>
/// Especialización CTI: Guantes. Serie PP.
/// TipoMedida = Punto por defecto (7–11 adulto, 3–6 infantil).
/// </summary>
public class etProductoGuante
{
    public Guid IdProducto { get; set; }
    public string? NbPalma { get; set; }
    public string? DsComposicion { get; set; }
    public string? ClMsCode { get; set; }
    public string? ClIndicePalma { get; set; }
    public string? DsForro { get; set; }
    public string? DsCierre { get; set; }
    public string? DsHomologacion { get; set; }
    // ✅ ClTipoMedida movido a etProducto (base)

    // ── Navegación ──
    public Producto Producto { get; set; } = null!;
}
