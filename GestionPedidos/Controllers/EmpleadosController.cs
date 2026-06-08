using GestionPedidos.Contracts.Empleados;
using GestionPedidos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionPedidos.Controllers;

[ApiController]
[Route("api/empleados")]
[Authorize]
public class EmpleadosController(IEmpleadoService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var empleados = await service.ObtenerTodosAsync();
        return Ok(empleados);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var empleado = await service.ObtenerPorIdAsync(id);
        if (empleado == null) return NotFound();
        return Ok(empleado);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmpleadoCreateDto dto)
    {
        var email = User.Identity?.Name ?? "Desconocido";
        var creado = await service.CrearAsync(dto, email);
        return CreatedAtAction(nameof(GetById), new { id = creado.IdEmpleado }, creado);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EmpleadoUpdateDto dto)
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
