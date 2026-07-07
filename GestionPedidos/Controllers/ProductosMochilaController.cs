using GestionPedidos.Contracts.Productos;
using GestionPedidos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionPedidos.Controllers;

[ApiController]
[Route("api/productos/mochila")]
[Authorize]
public class ProductosMochilaController(IProductoMochilaService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await service.ObtenerTodosAsync();
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await service.ObtenerPorIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductoMochilaCreateDto dto)
    {
        var email = User.Identity?.Name ?? "Desconocido";
        var creado = await service.CrearAsync(dto, email);
        return CreatedAtAction(nameof(GetById), new { id = creado.IdProducto }, creado);
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateBulk([FromBody] IEnumerable<ProductoMochilaBulkDto> dtos)
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
            return BadRequest(new { message = "Errores de validación en la carga masiva.", errores = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductoMochilaUpdateDto dto)
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
