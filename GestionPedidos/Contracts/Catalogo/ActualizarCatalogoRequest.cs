using System.ComponentModel.DataAnnotations;

namespace GestionPedidos.Contracts.Catalogo;

public class ActualizarCatalogoRequest
{
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
    public string? NbCatalogo { get; set; }

    [StringLength(255, ErrorMessage = "La descripción no puede exceder los 255 caracteres.")]
    public string? DsCatalogo { get; set; }

    public int? IdCatalogoPadre { get; set; }

    [RegularExpression("ACTIVO|INACTIVO", ErrorMessage = "El estatus debe ser ACTIVO o INACTIVO.")]
    public string? ClEstatusCatalogo { get; set; }
}
