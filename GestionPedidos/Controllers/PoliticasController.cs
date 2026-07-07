using GestionPedidos.Contracts.Precios;
using GestionPedidos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionPedidos.Controllers;

/// <summary>
/// Controlador para gestionar las políticas de precio y asignar clientes a ellas.
/// Solo el Administrador tiene acceso a esto.
/// </summary>
[ApiController]
[Route("api/politicas")]
[Authorize(Roles = "Admin")]
public class PoliticasController(IPoliticaService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var politicas = await service.ObtenerTodasAsync();
        return Ok(politicas);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PoliticaUpsertDto dto)
    {
        var userEmail = User.Identity?.Name ?? "Admin";
        var politica = await service.CrearAsync(dto, userEmail);
        return Ok(politica);
    }

    [HttpGet("{idPolitica:guid}/clientes")]
    public async Task<IActionResult> GetClientes(Guid idPolitica)
    {
        var clientes = await service.ObtenerClientesDePoliticaAsync(idPolitica);
        return Ok(clientes);
    }

    [HttpPost("{idPolitica:guid}/clientes")]
    public async Task<IActionResult> AsignarCliente(Guid idPolitica, [FromBody] ClientePoliticaUpsertDto dto)
    {
        if (idPolitica != dto.IdPolitica)
            return BadRequest(new { message = "El ID de la política en la ruta no coincide con el cuerpo de la petición." });

        var userEmail = User.Identity?.Name ?? "Admin";
        var asignacion = await service.AsignarClienteAsync(dto, userEmail);
        return Ok(asignacion);
    }

    [HttpDelete("{idPolitica:guid}/clientes/{idCliente:guid}")]
    public async Task<IActionResult> RemoverCliente(Guid idPolitica, Guid idCliente)
    {
        var removido = await service.RemoverClienteAsync(idPolitica, idCliente);
        if (!removido)
            return NotFound(new { message = "El cliente no está asignado a esta política." });

        return NoContent();
    }
}
