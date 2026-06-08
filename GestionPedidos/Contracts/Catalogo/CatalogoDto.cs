namespace GestionPedidos.Contracts.Catalogo;

/// <summary>
/// DTO de lectura para los catálogos maestros.
/// Incluye la cantidad de elementos para dar contexto rápido en listas admin.
/// </summary>
public sealed record CatalogoDto
{
    public int IdCatalogo { get; init; }
    public string ClCatalogo { get; init; } = null!;
    public string NbCatalogo { get; init; } = null!;
    public string? DsCatalogo { get; init; }
    public int? IdCatalogoPadre { get; init; }
    public string ClEstatusCatalogo { get; init; } = null!;
    public int TotalElementos { get; init; }
}
