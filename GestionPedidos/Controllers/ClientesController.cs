using GestionPedidos.Contracts.Clientes;
using GestionPedidos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionPedidos.Controllers;

[ApiController]
[Route("api/clientes")]
[Authorize]
public class ClientesController(IClienteService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clientes = await service.ObtenerTodosAsync();
        return Ok(clientes);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var cliente = await service.ObtenerPorIdAsync(id);
        if (cliente == null) return NotFound();
        return Ok(cliente);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ClienteCreateDto dto)
    {
        var email = User.Identity?.Name ?? "Desconocido";
        var creado = await service.CrearAsync(dto, email);
        return CreatedAtAction(nameof(GetById), new { id = creado.IdCliente }, creado);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ClienteUpdateDto dto)
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
