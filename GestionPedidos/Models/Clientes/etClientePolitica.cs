namespace GestionPedidos.Models;

/// <summary>
/// Asignación de un Cliente a una Política de Precio.
/// Un cliente puede pertenecer a múltiples políticas (ej: DISTRIBUIDORES_ES + VIP_ESPAÑA).
/// El motor de precios evaluará todas las políticas del cliente y usará la de mayor prioridad (NoPrioridad).
/// </summary>
public class etClientePolitica
{
    public Guid IdCliente { get; set; }
    public Guid IdPolitica { get; set; }

    /// <summary>
    /// Indica si esta es la política principal del cliente.
    /// Solo informativo; la prioridad real se lee de etPoliticaPrecio.NoPrioridad.
    /// </summary>
    public bool EsPrincipal { get; set; } = false;

    // Auditoría
    public required string ClOperadorCrea { get; set; }
    public string? ClOperadorModifica { get; set; }
    public required string NbArtefactoCrea { get; set; }
    public string? NbArtefactoModifica { get; set; }
    public DateTimeOffset FeCreacion { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FeModificacion { get; set; }

    // ── Navegación ──
    public etCliente Cliente { get; set; } = null!;
    public etPoliticaPrecio Politica { get; set; } = null!;
}
