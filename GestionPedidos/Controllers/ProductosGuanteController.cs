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

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var guante = await service.ObtenerPorIdAsync(id);
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
