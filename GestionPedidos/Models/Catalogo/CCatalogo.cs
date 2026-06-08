namespace GestionPedidos.Models.Catalogo;

/// <summary>
/// Cat�logo maestro del sistema.
/// Define qu� cat�logos existen (PAISES, AREAS, TIPOS_DOCUMENTO, etc.)
/// Soporta jerarqu�a padre-hijo.
/// </summary>
public class CCatalogo
{
    public int IdCatalogo { get; set; }
    public required string ClCatalogo { get; set; } // Clave �nica: PAISES, AREAS, ESTATUS, etc.
    public required string NbCatalogo { get; set; } // Nombre: "Pa�ses", "�reas", "Estados", etc.
    public string? DsCatalogo { get; set; } // Descripci�n
    public int? IdCatalogoPadre { get; set; } // Jerarqu�a (si aplica)
    public string ClEstatusCatalogo { get; set; } = "ACTIVO"; // ACTIVO | INACTIVO | ELIMINADO

    // Auditor�a
    public required string ClOperadorCrea { get; set; } // Usuario que cre�
    public string? ClOperadorModifica { get; set; } // Usuario que modific�
    public required string NbArtefactoCrea { get; set; } // Programa/m�dulo que cre�
    public string? NbArtefactoModifica { get; set; } // Programa/m�dulo que modific�
    public DateTimeOffset FeCreacion { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FeModificacion { get; set; }

    // -- Navegaci�n --
    public CCatalogo? CatalogoPadre { get; set; }
    public ICollection<CCatalogo> CatalogosHijos { get; set; } = [];
    public ICollection<CCatalogoElemento> Elementos { get; set; } = [];
}
