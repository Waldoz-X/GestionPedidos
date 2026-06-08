using GestionPedidos.Contracts.Empleados;
using GestionPedidos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionPedidos.Controllers;

/// <summary>
/// Gestionar asignaciones entre empleados y clientes
/// </summary>
[ApiController]
[Route("api/asignaciones")]
[Authorize]
public class AsignacionesController(IAsignacionService service) : ControllerBase
{
    /// <summary>
    /// Obtener todas las asignaciones empleado-cliente
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager,EMPLEADO")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var asignaciones = await service.ObtenerTodosAsync();
            return Ok(asignaciones);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error al obtener asignaciones", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener asignación específica por empleado y cliente
    /// </summary>
    [HttpGet("{idEmpleado:guid}/{idCliente:guid}")]
    [Authorize(Roles = "Admin,Manager,EMPLEADO")]
    public async Task<IActionResult> GetById(Guid idEmpleado, Guid idCliente)
    {
        try
        {
            var asignacion = await service.ObtenerPorIdAsync(idEmpleado, idCliente);
            if (asignacion == null) 
                return NotFound(new { message = "Asignación no encontrada" });
            return Ok(asignacion);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error al obtener asignación", error = ex.Message });
        }
    }

    /// <summary>
    /// Crear nueva asignación empleado-cliente
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] AsignacionClienteEmpleadoCreateDto dto)
    {
        try
        {
            var email = User.Identity?.Name ?? "Desconocido";
            var creada = await service.CrearAsync(dto, email);
            return CreatedAtAction(nameof(GetById), 
                new { idEmpleado = creada.IdEmpleado, idCliente = creada.IdCliente }, 
                creada);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error al crear asignación", error = ex.Message });
        }
    }

    /// <summary>
    /// Actualizar asignación existente
    /// </summary>
    [HttpPut("{idEmpleado:guid}/{idCliente:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(Guid idEmpleado, Guid idCliente, [FromBody] AsignacionClienteEmpleadoUpdateDto dto)
    {
        try
        {
            var email = User.Identity?.Name ?? "Desconocido";
            var actualizada = await service.ActualizarAsync(idEmpleado, idCliente, dto, email);
            if (actualizada == null) 
                return NotFound(new { message = "Asignación no encontrada" });
            return Ok(actualizada);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error al actualizar asignación", error = ex.Message });
        }
    }

    /// <summary>
    /// Eliminar asignación empleado-cliente
    /// </summary>
    [HttpDelete("{idEmpleado:guid}/{idCliente:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(Guid idEmpleado, Guid idCliente)
    {
        try
        {
            var eliminada = await service.EliminarAsync(idEmpleado, idCliente);
            if (!eliminada) 
                return NotFound(new { message = "Asignación no encontrada" });
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error al eliminar asignación", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener todos los clientes de un empleado
    /// </summary>
    [HttpGet("empleado/{idEmpleado:guid}/clientes")]
    [Authorize(Roles = "Admin,Manager,EMPLEADO")]
    public async Task<IActionResult> GetClientesDelEmpleado(Guid idEmpleado)
    {
        try
        {
            var clientes = await service.ObtenerClientesDelEmpleadoAsync(idEmpleado);
            return Ok(clientes);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error al obtener clientes", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener todos los empleados asignados a un cliente
    /// </summary>
    [HttpGet("cliente/{idCliente:guid}/empleados")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetEmpleadosDelCliente(Guid idCliente)
    {
        try
        {
            var empleados = await service.ObtenerEmpleadosDelClienteAsync(idCliente);
            return Ok(empleados);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error al obtener empleados", error = ex.Message });
        }
    }

    /// <summary>
    /// Verificar si un empleado tiene acceso a un cliente
    /// </summary>
    [HttpGet("verificar/{idEmpleado:guid}/{idCliente:guid}/acceso")]
    [Authorize(Roles = "Admin,Manager,EMPLEADO")]
    public async Task<IActionResult> VerificarAcceso(Guid idEmpleado, Guid idCliente)
    {
        try
        {
            var tieneAcceso = await service.EmpleadoTieneAccesoAClienteAsync(idEmpleado, idCliente);
            return Ok(new { idEmpleado, idCliente, tieneAcceso });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error al verificar acceso", error = ex.Message });
        }
    }
}


