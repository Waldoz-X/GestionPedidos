namespace GestionPedidos.Models;

/// <summary>
/// Pedido. Estatus: BORRADOR → CONFIRMADO → FACTURADO → ENVIADO | CANCELADO.
/// Total = Subtotal - DescuentoComercial - DescuentoAdmin (computado por trigger/app).
/// Ahora incluye IdDireccionEnvio (v4.1).
/// </summary>
public class etPedido
{
    public Guid IdPedido { get; set; }
    public Guid IdCliente { get; set; }
    public Guid IdUsuarioCaptura { get; set; }
    public Guid? IdDireccionEnvio { get; set; }
    public Guid? IdPolitica { get; set; }
    public string ClFolio { get; set; } = null!;
    public string ClEstatusPedido { get; set; } = null!;
    public Moneda ClMoneda { get; set; } = null!;
    public decimal MnSubtotal { get; set; }
    public decimal MnDescuentoComercial { get; set; }
    public decimal MnDescuentoAdmin { get; set; }
    public decimal MnTotal { get; set; }
    public DateTimeOffset FePedido { get; set; }
    public DateTimeOffset? FeExpiracion { get; set; }

    // Auditoría
    public required string ClOperadorCrea { get; set; }
    public string? ClOperadorModifica { get; set; }
    public required string NbArtefactoCrea { get; set; }
    public string? NbArtefactoModifica { get; set; }
    public DateTimeOffset FeCreacion { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FeModificacion { get; set; }

    // ── Navegación ──
    public Cliente Cliente { get; set; } = null!;
    public Usuario UsuarioCaptura { get; set; } = null!;
    public DireccionCliente? DireccionEnvio { get; set; }
    public PoliticaPrecio? Politica { get; set; }
    public ICollection<LineaPedido> Lineas { get; set; } = [];
    public ICollection<HistorialPedido> Historial { get; set; } = [];
}
