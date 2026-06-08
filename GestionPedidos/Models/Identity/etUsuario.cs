using Microsoft.AspNetCore.Identity;

namespace GestionPedidos.Models;

/// <summary>
/// Usuario del sistema. Extiende IdentityUser para autenticación.
/// Los roles se gestionan en AspNetRoles/AspNetUserRoles.
/// </summary>
public class etUsuario : IdentityUser<Guid>
{
    public Guid? IdCliente { get; set; }
    public string ClEstatusUsuario { get; set; } = "ACTIVO";
    public DateTimeOffset FeCreacion { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FeModificacion { get; set; }

    // ── Navegación ──
    public Cliente? Cliente { get; set; }
    public Empleado? Empleado { get; set; }
    public ICollection<Pedido> PedidosCapturados { get; set; } = [];
    public ICollection<HistorialPrecio> HistorialPrecios { get; set; } = [];
    public ICollection<HistorialPedido> HistorialPedidos { get; set; } = [];
}
