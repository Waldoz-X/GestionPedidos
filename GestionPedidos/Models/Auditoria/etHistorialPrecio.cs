namespace GestionPedidos.Models;

/// <summary>
/// Registro INMUTABLE de cambios de precio. Nunca UPDATE ni DELETE.
/// </summary>
public class etHistorialPrecio
{
    public Guid Id { get; set; }
    public Guid IdPrecio { get; set; }
    public decimal PrecioAnterior { get; set; }
    public decimal PrecioNuevo { get; set; }
    public Guid IdUsuario { get; set; }
    public DateTimeOffset RegistradoEn { get; set; } = DateTimeOffset.UtcNow;

    // ── Navegación ──
    public Precio Precio { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}
