using System.Security.Claims;
using GestionPedidos.Contracts.Catalogo;
using GestionPedidos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionPedidos.Controllers;

/// <summary>
/// Controlador universal de catálogos genéricos.
/// 
/// ┌─────────────────────────────────────────────────────────────────┐
/// │  GET    /api/catalogos              → Lista catálogos maestros  │
/// │  GET    /api/catalogos/GAMAS        → Elementos del catálogo   │
/// │  GET    /api/catalogos/elementos/5  → Elemento por ID          │
/// │  POST   /api/catalogos/GAMAS       → Crear elemento            │
/// │  PUT    /api/catalogos/elementos/5 → Actualizar elemento       │
/// │  DELETE /api/catalogos/elementos/5 → Soft-delete               │
/// └─────────────────────────────────────────────────────────────────┘
/// 
/// El frontend solo necesita saber la clave del catálogo (GAMAS, DIVISIONES,
/// AREAS, SERIES, etc.) para llenar cualquier dropdown/selector.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CatalogosController : ControllerBase
{
    private readonly ICatalogoService _catalogoService;

    public CatalogosController(ICatalogoService catalogoService)
    {
        _catalogoService = catalogoService;
    }

    // ── Catálogos maestros ──────────────────────────────────────────

    /// <summary>
    /// Lista todos los catálogos maestros activos del sistema.
    /// Útil para pantallas de administración donde se necesita ver qué catálogos existen.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetCatalogos()
    {
        var catalogos = await _catalogoService.GetCatalogosAsync();
        return Ok(catalogos);
    }

    /// <summary>
    /// Crea un nuevo catálogo maestro (padre).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CrearCatalogo([FromBody] CrearCatalogoRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var creado = await _catalogoService.CrearCatalogoAsync(request, ObtenerOperador());
            return CreatedAtAction(nameof(GetCatalogos), new { id = creado.IdCatalogo }, creado);
        }
        catch (ArgumentException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza un catálogo maestro.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ActualizarCatalogo(int id, [FromBody] ActualizarCatalogoRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var actualizado = await _catalogoService.ActualizarCatalogoAsync(id, request, ObtenerOperador());
        return actualizado is null
            ? NotFound(new { mensaje = $"No se encontró el catálogo con ID {id}." })
            : Ok(actualizado);
    }

    /// <summary>
    /// Eliminación lógica de un catálogo maestro.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EliminarCatalogo(int id)
    {
        var eliminado = await _catalogoService.EliminarCatalogoAsync(id, ObtenerOperador());
        return eliminado
            ? NoContent()
            : NotFound(new { mensaje = $"No se encontró el catálogo con ID {id}." });
    }

    // ── Elementos (endpoint universal) ─────────────────────────────

    /// <summary>
    /// Devuelve los elementos activos de un catálogo dado su código.
    /// Ejemplo: GET /api/catalogos/GAMAS → [{ "clCatalogoElemento": "PRO", ... }, ...]
    /// </summary>
    [HttpGet("{clCatalogo}")]
    [Authorize]
    public async Task<IActionResult> GetElementos(string clCatalogo)
    {
        var elementos = await _catalogoService.GetElementosAsync(clCatalogo.ToUpperInvariant());
        return Ok(elementos);
    }

    /// <summary>
    /// Obtiene un elemento específico por su ID.
    /// </summary>
    [HttpGet("elementos/{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetElementoById(int id)
    {
        var elemento = await _catalogoService.GetElementoByIdAsync(id);
        return elemento is null
            ? NotFound(new { mensaje = $"No se encontró el elemento con ID {id}." })
            : Ok(elemento);
    }

    /// <summary>
    /// Crea un nuevo elemento dentro del catálogo especificado.
    /// Ejemplo: POST /api/catalogos/GAMAS { "clCatalogoElemento": "ELITE", "nbCatalogoElemento": "Elite" }
    /// </summary>
    [HttpPost("{clCatalogo}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CrearElemento(string clCatalogo, [FromBody] CrearCatalogoElementoRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var operador = ObtenerOperador();

        try
        {
            var creado = await _catalogoService.CrearElementoAsync(clCatalogo.ToUpperInvariant(), request, operador);
            return CreatedAtAction(
                nameof(GetElementoById),
                new { id = creado.IdCatalogoElemento },
                creado);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza un elemento existente. Solo se sobreescriben los campos enviados.
    /// </summary>
    [HttpPut("elementos/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ActualizarElemento(int id, [FromBody] ActualizarCatalogoElementoRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var operador = ObtenerOperador();
        var actualizado = await _catalogoService.ActualizarElementoAsync(id, request, operador);

        return actualizado is null
            ? NotFound(new { mensaje = $"No se encontró el elemento con ID {id}." })
            : Ok(actualizado);
    }

    /// <summary>
    /// Eliminación lógica (soft-delete). Cambia estatus a INACTIVO.
    /// </summary>
    [HttpDelete("elementos/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EliminarElemento(int id)
    {
        var operador = ObtenerOperador();
        var eliminado = await _catalogoService.EliminarElementoAsync(id, operador);

        return eliminado
            ? NoContent()
            : NotFound(new { mensaje = $"No se encontró el elemento con ID {id}." });
    }

    // ── Helpers ─────────────────────────────────────────────────────

    private string ObtenerOperador()
    {
        return User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? "SISTEMA";
    }
}
