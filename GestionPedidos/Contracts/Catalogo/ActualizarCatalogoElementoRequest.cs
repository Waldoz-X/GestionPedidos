using System.ComponentModel.DataAnnotations;

namespace GestionPedidos.Contracts.Catalogo;

/// <summary>
/// DTO para actualizar un elemento existente de un catálogo.
/// Solo se actualizan los campos enviados (nombre, descripción, padre, estatus).
/// </summary>
public sealed record ActualizarCatalogoElementoRequest
{
    [MaxLength(150)]
    public string? NbCatalogoElemento { get; init; }

    [MaxLength(500)]
    public string? DsCatalogoElemento { get; init; }

    public int? IdCatalogoElementoPadre { get; init; }

    /// <summary>
    /// Permite activar/inactivar sin eliminar físicamente.
    /// Valores válidos: ACTIVO, INACTIVO.
    /// </summary>
    [MaxLength(20)]
    public string? ClEstatusCatalogoElemento { get; init; }
}
