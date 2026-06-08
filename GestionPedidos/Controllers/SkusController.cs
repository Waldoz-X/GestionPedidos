using GestionPedidos.Contracts.Skus;
using GestionPedidos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionPedidos.Controllers;

[ApiController]
[Route("api/skus")]
[Authorize]
public class SkusController(ISkuService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var skus = await service.ObtenerTodosAsync();
        return Ok(skus);
    }

    [HttpGet("variante/{idVariante:guid}")]
    public async Task<IActionResult> GetByVariante(Guid idVariante)
    {
        var skus = await service.ObtenerPorVarianteAsync(idVariante);
        return Ok(skus);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var sku = await service.ObtenerPorIdAsync(id);
        if (sku == null) return NotFound();
        return Ok(sku);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SkuCreateDto dto)
    {
        var email = User.Identity?.Name ?? "Desconocido";
        var creado = await service.CrearAsync(dto, email);
        return CreatedAtAction(nameof(GetById), new { id = creado.IdSku }, creado);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SkuUpdateDto dto)
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
