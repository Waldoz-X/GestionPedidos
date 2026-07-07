namespace GestionPedidos.Models;

/// <summary>
/// Cliente comercial. Tipo: DISTRIBUIDOR | DIRECTO | ONLINE.
/// Moneda: USD | MXN | EUR.
/// </summary>
public class etCliente
{
    public Guid IdCliente { get; set; }
    public string NbComercial { get; set; } = null!;
    public string ClTipoCliente { get; set; } = null!;
    
    // --- Moneda ---
    public int IdElemMoneda { get; set; }
    public Moneda ClMoneda { get; set; } = null!;
    
    public string? DsCanalVenta { get; set; }
    public decimal MnLimiteCredito { get; set; }
    public string ClEstatusCliente { get; set; } = "ACTIVO";

    /// <summary>
    /// Región geográfica del cliente. Define qué catálogo de productos puede ver.
    /// Valores: "MX" (México/MXN), "ES" (España/EUR), "GLOBAL" (ve todo).
    /// </summary>
    public string ClRegion { get; set; } = "MX";

    // Auditoría
    public required string ClOperadorCrea { get; set; }
    public string? ClOperadorModifica { get; set; }
    public required string NbArtefactoCrea { get; set; }
    public string? NbArtefactoModifica { get; set; }
    public DateTimeOffset FeCreacion { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FeModificacion { get; set; }

    // ── Navegación ──
    public ICollection<Usuario> Usuarios { get; set; } = [];
    public ICollection<DireccionCliente> Direcciones { get; set; } = [];
    public ICollection<Pedido> Pedidos { get; set; } = [];
    public ICollection<Precio> Precios { get; set; } = [];
    public ICollection<VisibilidadCatalogo> Visibilidades { get; set; } = [];
    public ICollection<etClientePolitica> Politicas { get; set; } = [];
    public ICollection<AsignacionClienteEmpleado> AsignacionesEmpleado { get; set; } = [];
}
