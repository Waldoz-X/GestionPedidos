namespace GestionPedidos.Models;

/// <summary>
/// Especialización CTI: Textiles. Serie TX.
/// GramajeGsm permite queries: WHERE GramajeGsm BETWEEN 180 AND 300.
/// </summary>
public class etProductoTextil
{
    public Guid IdProducto { get; set; }
    public string NbTejido { get; set; } = null!;
    public string? DsComposicion { get; set; }
    public string? DsCorte { get; set; }
    public int? NoGramajeGsm { get; set; }
    public Genero ClGenero { get; set; }
    // ✅ ClTipoMedida movido a etProducto (base)

    // ── Navegación ──
    public Producto Producto { get; set; } = null!;
}
