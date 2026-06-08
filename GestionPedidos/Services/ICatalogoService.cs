using GestionPedidos.Contracts.Catalogo;

namespace GestionPedidos.Services;

/// <summary>
/// Contrato para el servicio dinámico de catálogos genéricos.
/// Un solo servicio gobierna todos los catálogos (GAMAS, DIVISIONES, AREAS, etc.)
/// </summary>
public interface ICatalogoService
{
    // ── Catálogos maestros ──
    Task<IReadOnlyList<CatalogoDto>> GetCatalogosAsync();
    Task<CatalogoDto> CrearCatalogoAsync(CrearCatalogoRequest request, string operador);
    Task<CatalogoDto?> ActualizarCatalogoAsync(int idCatalogo, ActualizarCatalogoRequest request, string operador);
    Task<bool> EliminarCatalogoAsync(int idCatalogo, string operador);

    // ── Elementos (el endpoint universal) ──
    Task<IReadOnlyList<CatalogoElementoDto>> GetElementosAsync(string clCatalogo);
    Task<CatalogoElementoDto?> GetElementoByIdAsync(int idElemento);
    Task<CatalogoElementoDto> CrearElementoAsync(string clCatalogo, CrearCatalogoElementoRequest request, string operador);
    Task<CatalogoElementoDto?> ActualizarElementoAsync(int idElemento, ActualizarCatalogoElementoRequest request, string operador);
    Task<bool> EliminarElementoAsync(int idElemento, string operador);
}
