using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GestionPedidos.Contracts.Inventario;
using GestionPedidos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionPedidos.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventarioController : ControllerBase
{
    private readonly IInventarioService inventarioService;

    public InventarioController(IInventarioService inventarioService)
    {
        this.inventarioService = inventarioService;
    }

    [HttpPost("entrada")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RegistrarEntrada([FromBody] RegistrarMovimientoRequest request)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? "Desconocido";
        try
        {
            var movimiento = await inventarioService.RegistrarEntradaAsync(request, userEmail);
            return Ok(movimiento);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("baja")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RegistrarBaja([FromBody] RegistrarMovimientoRequest request)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? "Desconocido";
        try
        {
            var movimiento = await inventarioService.RegistrarBajaAsync(request, userEmail);
            return Ok(movimiento);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("ajuste")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RegistrarAjuste([FromBody] RegistrarAjusteRequest request)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? "Desconocido";
        try
        {
            var movimiento = await inventarioService.RegistrarAjusteAsync(request, userEmail);
            return Ok(movimiento);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("kardex/{idSku:guid}")]
    [Authorize(Roles = "Admin,Manager,EMPLEADO")]
    public async Task<IActionResult> ObtenerKardex(Guid idSku)
    {
        var kardex = await inventarioService.ObtenerKardexSkuAsync(idSku);
        return Ok(kardex);
    }

    [HttpGet("libro-auditoria")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ObtenerLibroAuditoria(
        [FromQuery] string? clTipoMovimiento,
        [FromQuery] DateTimeOffset? feInicio,
        [FromQuery] DateTimeOffset? feFin)
    {
        var libro = await inventarioService.ObtenerLibroAuditoriaAsync(clTipoMovimiento, feInicio, feFin);
        return Ok(libro);
    }

    [HttpGet("stock-real")]
    [Authorize(Roles = "Admin,Manager,EMPLEADO")]
    public async Task<IActionResult> ObtenerStockReal()
    {
        var stock = await inventarioService.ObtenerStockRealAsync();
        return Ok(stock);
    }

    [HttpGet("rotacion")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ObtenerRotacion(
        [FromQuery] DateTimeOffset? feInicio,
        [FromQuery] DateTimeOffset? feFin)
    {
        if (!feInicio.HasValue || !feFin.HasValue)
        {
            return BadRequest(new { mensaje = "Los parametros 'feInicio' y 'feFin' son obligatorios." });
        }

        var rotacion = await inventarioService.ObtenerRotacionInventarioAsync(feInicio.Value, feFin.Value);
        return Ok(rotacion);
    }
}
