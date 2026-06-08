using System.ComponentModel.DataAnnotations;

namespace GestionPedidos.Contracts.Catalogo;

/// <summary>
/// DTO para crear un nuevo elemento dentro de un catálogo.
/// El backend asigna auditoría automáticamente.
/// </summary>
public sealed record CrearCatalogoElementoRequest
{
    [Required, MaxLength(50)]
    public string ClCatalogoElemento { get; init; } = null!;

    [Required, MaxLength(150)]
    public string NbCatalogoElemento { get; init; } = null!;

    [MaxLength(500)]
    public string? DsCatalogoElemento { get; init; }

    /// <summary>
    /// ID del elemento padre, para jerarquías dentro del mismo catálogo.
    /// Null si es un elemento raíz.
    /// </summary>
    public int? IdCatalogoElementoPadre { get; init; }
}
