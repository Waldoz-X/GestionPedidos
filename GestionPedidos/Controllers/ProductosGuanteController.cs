using GestionPedidos.Contracts.Productos;
using GestionPedidos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionPedidos.Controllers;

[ApiController]
[Route("api/productos/guantes")]
[Authorize] // Protegido por JWT
public class ProductosGuanteController(IProductoGuanteService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var guantes = await service.ObtenerTodosAsync();
        return Ok(guantes);
    }

    [HttpGet("catalogo")]
    public async Task<IActionResult> GetCatalogoPaginado([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        Guid? idCliente = null;
        if (User.IsInRole("CLIENTE"))
        {
            var idClaim = User.FindFirst("idCliente")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.UserData)?.Value;
            if (!string.IsNullOrEmpty(idClaim) && Guid.TryParse(idClaim, out var parsedId))
            {
                idCliente = parsedId;
            }
        }
        var result = await service.ObtenerCatalogoPaginadoAsync(page, pageSize, idCliente);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        Guid? idCliente = null;
        if (User.IsInRole("CLIENTE"))
        {
            var idClaim = User.FindFirst("idCliente")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.UserData)?.Value;
            if (!string.IsNullOrEmpty(idClaim) && Guid.TryParse(idClaim, out var parsedId))
            {
                idCliente = parsedId;
            }
        }
        var guante = await service.ObtenerPorIdAsync(id, idCliente);
        if (guante == null) return NotFound();
        return Ok(guante);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductoGuanteCreateDto dto)
    {
        var email = User.Identity?.Name ?? "Desconocido";
        var creado = await service.CrearAsync(dto, email);
        return CreatedAtAction(nameof(GetById), new { id = creado.IdProducto }, creado);
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateBulk([FromBody] IEnumerable<ProductoGuanteBulkDto> dtos)
    {
        if (dtos == null || !dtos.Any())
        {
            return BadRequest("El arreglo de productos no puede estar vacío.");
        }

        var email = User.Identity?.Name ?? "SISTEMA_BULK";

        try
        {
            var cantidad = await service.CrearMasivoAsync(dtos, email);
            return Ok(new { message = $"Se han insertado {cantidad} productos exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            // Errores de mapeo (claves no encontradas en catálogos)
            return BadRequest(new { message = "Errores de validación en la carga masiva.", errores = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductoGuanteUpdateDto dto)
    {
        var email = User.Identity?.Name ?? "Desconocido";
        var actualizado = await service.ActualizarAsync(id, dto, email);
        if (actualizado == null) return NotFound();
        return Ok(actualizado);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var eliminado = await service.EliminarAsync(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }
}
