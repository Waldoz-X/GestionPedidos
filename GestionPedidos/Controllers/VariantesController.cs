using GestionPedidos.Contracts.Variantes;
using GestionPedidos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionPedidos.Controllers;

[ApiController]
[Route("api/variantes")]
[Authorize]
public class VariantesController(IVarianteService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var variantes = await service.ObtenerTodosAsync();
        return Ok(variantes);
    }

    [HttpGet("producto/{idProducto:guid}")]
    public async Task<IActionResult> GetByProducto(Guid idProducto)
    {
        var variantes = await service.ObtenerPorProductoAsync(idProducto);
        return Ok(variantes);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var variante = await service.ObtenerPorIdAsync(id);
        if (variante == null) return NotFound();
        return Ok(variante);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VarianteCreateDto dto)
    {
        var email = User.Identity?.Name ?? "Desconocido";
        var creado = await service.CrearAsync(dto, email);
        return CreatedAtAction(nameof(GetById), new { id = creado.IdVariante }, creado);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] VarianteUpdateDto dto)
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
