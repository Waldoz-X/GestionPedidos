namespace GestionPedidos.Contracts.Catalogo;

/// <summary>
/// DTO de lectura para elementos del catálogo.
/// Se usa para llenar dropdowns/selectores en el frontend.
/// No expone campos de auditoría.
/// </summary>
public sealed record CatalogoElementoDto
{
    public int IdCatalogoElemento { get; init; }
    public string ClCatalogoElemento { get; init; } = null!;
    public string NbCatalogoElemento { get; init; } = null!;
    public string? DsCatalogoElemento { get; init; }
    public int? IdCatalogoElementoPadre { get; init; }
    
    // Agregados para mostrar información en tablas
    public string? NbCatalogo { get; init; } // Nombre del catálogo maestro (ej. "Áreas")
    public string? NbCatalogoElementoPadre { get; init; } // Nombre del elemento padre (si aplica)
    public string? ClEstatusCatalogoElemento { get; init; } // Estatus del elemento (ACTIVO, INACTIVO)
}
