namespace GestionPedidos.Models.Catalogo;

/// <summary>
/// Elemento de un cat�logo.
/// Define los valores espec�ficos de un cat�logo (ESP, ARG para PAISES; ADMIN, USUARIO para ROLES, etc.)
/// Soporta jerarqu�a padre-hijo dentro del mismo cat�logo.
/// </summary>
public class CCatalogoElemento
{
    public int IdCatalogoElemento { get; set; }
    public int IdCatalogo { get; set; } // FK a CCatalogo
    public required string ClCatalogoElemento { get; set; } // Clave �nica: ESP, ARG, ADMIN, etc.
    public required string NbCatalogoElemento { get; set; } // Nombre: "Espa�a", "Argentina", "Administrador", etc.
    public string? DsCatalogoElemento { get; set; } // Descripci�n
    public int? IdCatalogoElementoPadre { get; set; } // Jerarqu�a dentro del mismo cat�logo
    public string ClEstatusCatalogoElemento { get; set; } = "ACTIVO"; // ACTIVO | INACTIVO | ELIMINADO

    // Auditor�a
    public required string ClOperadorCrea { get; set; }
    public string? ClOperadorModifica { get; set; }
    public required string NbArtefactoCrea { get; set; }
    public string? NbArtefactoModifica { get; set; }
    public DateTimeOffset FeCreacion { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FeModificacion { get; set; }

    // -- Navegaci�n --
    public CCatalogo Catalogo { get; set; } = null!;
    public CCatalogoElemento? ElementoPadre { get; set; }
    public ICollection<CCatalogoElemento> ElementosHijos { get; set; } = new List<CCatalogoElemento>();
}
