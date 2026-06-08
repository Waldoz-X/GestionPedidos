using System.ComponentModel.DataAnnotations;

namespace GestionPedidos.Contracts.Catalogo;

public class CrearCatalogoRequest
{
    [Required(ErrorMessage = "La clave del catálogo es obligatoria.")]
    [StringLength(50, ErrorMessage = "La clave no puede exceder los 50 caracteres.")]
    public required string ClCatalogo { get; set; }

    [Required(ErrorMessage = "El nombre del catálogo es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
    public required string NbCatalogo { get; set; }

    [StringLength(255, ErrorMessage = "La descripción no puede exceder los 255 caracteres.")]
    public string? DsCatalogo { get; set; }

    public int? IdCatalogoPadre { get; set; }
}
