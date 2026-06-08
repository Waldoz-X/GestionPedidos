namespace GestionPedidos.Models;

/// <summary>
/// NUEVO v4.1 — Registro INMUTABLE de transiciones de estado del pedido.
/// Permite auditar: quién confirmó, cuándo se envió, etc.
/// </summary>
public class etHistorialPedido
{
    public Guid Id { get; set; }
    public Guid IdPedido { get; set; }
    public EstatusPedido? EstatusAnterior { get; set; }
    public EstatusPedido EstatusNuevo { get; set; }
    public Guid IdUsuario { get; set; }
    public string? Notas { get; set; }
    public DateTimeOffset RegistradoEn { get; set; } = DateTimeOffset.UtcNow;

    // ── Navegación ──
    public Pedido Pedido { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}
