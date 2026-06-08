namespace GestionPedidos.Models;

/// <summary>
/// Especialización CTI: Fitness/Box. Serie FN.
/// TipoMedida = Talla por defecto (AXL, AL, AM).
/// </summary>
public class etProductoFitness
{
    public Guid IdProducto { get; set; }
    public string? DsComposicion { get; set; }
    public string? DsRelleno { get; set; }
    public string? DsCierre { get; set; }
    public string? DsProteccion { get; set; }
    public decimal? NoPesoOz { get; set; }
    // ✅ ClTipoMedida movido a etProducto (base)

    // ── Navegación ──
    public Producto Producto { get; set; } = null!;
}
