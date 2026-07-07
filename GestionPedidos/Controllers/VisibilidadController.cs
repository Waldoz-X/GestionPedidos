using GestionPedidos.Contracts.Visibilidad;
using GestionPedidos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GestionPedidos.Controllers;

/// <summary>
/// Control de Visibilidad de Catálogo — Whitelist por cliente.
/// Un producto es invisible a un cliente a menos que exista un registro VISIBLE o EXCLUSIVO.
/// </summary>
[ApiController]
[Route("api/visibilidad")]
[Authorize]
public class VisibilidadController(IVisibilidadService service) : ControllerBase
{
    private Guid ObtenerIdClienteDelJwt()
    {
        var claim = User.FindFirst("idCliente")?.Value
                    ?? User.FindFirst(ClaimTypes.UserData)?.Value;

        if (claim == null || !Guid.TryParse(claim, out var idCliente))
            throw new UnauthorizedAccessException("El usuario no tiene un cliente asociado.");

        return idCliente;
    }

    /// <summary>
    /// Lista los productos que el cliente loggeado puede ver.
    /// Whitelist estricta: solo productos con registro VISIBLE o EXCLUSIVO en el ACL.
    /// El IdCliente viene del JWT — Angular no puede pasarlo manualmente.
    /// </summary>
    [HttpGet("mis-productos")]
    public async Task<IActionResult> MisProductos()
    {
        try
        {
            var idCliente = ObtenerIdClienteDelJwt();
            var productos = await service.ObtenerProductosVisiblesAsync(idCliente);
            return Ok(productos);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Asigna o actualiza la visibilidad de un producto para un cliente.
    /// ClTipoAcceso válidos: "VISIBLE" | "EXCLUSIVO" | "OCULTO"
    /// Solo Admin puede ejecutar este endpoint.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AsignarVisibilidad([FromBody] VisibilidadUpsertDto dto)
    {
        var validos = new[] { "VISIBLE", "EXCLUSIVO", "OCULTO" };
        if (!validos.Contains(dto.ClTipoAcceso.ToUpperInvariant()))
            return BadRequest(new { message = $"ClTipoAcceso inválido. Valores válidos: {string.Join(", ", validos)}" });

        var userEmail = User.Identity?.Name ?? "Admin";
        var visibilidad = await service.AsignarVisibilidadAsync(dto, userEmail);
        return Ok(visibilidad);
    }

    /// <summary>
    /// Revoca el acceso de un cliente a un producto (elimina el registro del ACL).
    /// Solo Admin.
    /// </summary>
    [HttpDelete("{idCliente:guid}/{idProducto:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RevocarVisibilidad(Guid idCliente, Guid idProducto)
    {
        var eliminado = await service.RevocarVisibilidadAsync(idCliente, idProducto);
        if (!eliminado)
            return NotFound(new { message = "No existe registro de visibilidad para este cliente y producto." });

        return NoContent();
    }

    /// <summary>
    /// Lista qué clientes tienen acceso a un producto específico.
    /// Útil para el Admin para saber quién puede ver un producto exclusivo.
    /// Solo Admin.
    /// </summary>
    [HttpGet("producto/{idProducto:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ClientesDeProducto(Guid idProducto)
    {
        var clientes = await service.ObtenerClientesDeProductoAsync(idProducto);
        return Ok(clientes);
    }
}
