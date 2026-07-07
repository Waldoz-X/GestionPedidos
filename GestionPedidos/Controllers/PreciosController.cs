using GestionPedidos.Contracts.Precios;
using GestionPedidos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GestionPedidos.Controllers;

/// <summary>
/// Motor de Precios — Acceso controlado por JWT.
/// El cliente solo puede resolver sus propios precios.
/// El Admin puede ver, crear y cargar todos los precios.
/// </summary>
[ApiController]
[Route("api/precios")]
[Authorize]
public class PreciosController(IPrecioService service) : ControllerBase
{
    // ── Helpers privados ─────────────────────────────────────────────────────

    /// <summary>
    /// Obtiene el IdCliente del JWT. Lanza 401 si el usuario no tiene cliente asociado.
    /// NUNCA se acepta un idCliente del body — siempre del JWT para seguridad.
    /// </summary>
    private Guid ObtenerIdClienteDelJwt()
    {
        var claim = User.FindFirst("idCliente")?.Value
                    ?? User.FindFirst(ClaimTypes.UserData)?.Value;

        if (claim == null || !Guid.TryParse(claim, out var idCliente))
            throw new UnauthorizedAccessException("El usuario no tiene un cliente asociado.");

        return idCliente;
    }

    private Guid ObtenerUserIdDelJwt()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }

    // ── Endpoints de RESOLUCIÓN DE PRECIO (Cliente y Admin) ─────────────────

    /// <summary>
    /// Resuelve el precio final de un SKU para el cliente loggeado.
    /// Aplica el motor en cascada: Nivel 1 (directo) → Nivel 2 (política).
    /// </summary>
    [HttpGet("resolver/{idSku:guid}")]
    public async Task<IActionResult> ResolverPrecio(Guid idSku)
    {
        try
        {
            var idCliente = ObtenerIdClienteDelJwt();
            var resultado = await service.ResolverPrecioAsync(idSku, idCliente);
            return Ok(resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Resuelve precios de múltiples SKUs de una vez.
    /// Ideal para cargar el catálogo completo en Angular sin N llamadas individuales.
    /// </summary>
    [HttpPost("resolver/bulk")]
    public async Task<IActionResult> ResolverBulk([FromBody] List<Guid> idSkus)
    {
        if (idSkus == null || idSkus.Count == 0)
            return BadRequest(new { message = "La lista de SKUs no puede estar vacía." });

        try
        {
            var idCliente = ObtenerIdClienteDelJwt();
            var resultados = await service.ResolverBulkAsync(idSkus, idCliente);
            return Ok(resultados);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    // ── Endpoints de ADMINISTRACIÓN (Solo Admin) ─────────────────────────────

    /// <summary>Lista todos los precios configurados en el sistema.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var precios = await service.ObtenerTodosAsync();
        return Ok(precios);
    }

    /// <summary>Crea o actualiza un precio individual. Registra historial automáticamente.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Upsert([FromBody] PrecioUpsertDto dto)
    {
        var userEmail = User.Identity?.Name ?? "Admin";
        var userId = ObtenerUserIdDelJwt();
        var precio = await service.CrearOActualizarAsync(dto, userEmail, userId);
        return Ok(precio);
    }

    /// <summary>
    /// Carga masiva de precios desde JSON (generado por Python/Excel).
    /// Traduce ClItem/CodigoBarras → IdSku, NbComercial → IdCliente, NbPolitica → IdPolitica.
    /// Si algún mapeo falla, rechaza TODO el lote (atomicidad total).
    /// </summary>
    [HttpPost("bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CargarMasiva([FromBody] IEnumerable<PrecioBulkDto> dtos)
    {
        if (dtos == null || !dtos.Any())
            return BadRequest(new { message = "El arreglo de precios no puede estar vacío." });

        var userEmail = User.Identity?.Name ?? "Admin";
        var userId = ObtenerUserIdDelJwt();
        var resultado = await service.CargarMasivaAsync(dtos, userEmail, userId);

        if (resultado.Errores.Count > 0)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    /// <summary>Historial inmutable de cambios de un precio específico.</summary>
    [HttpGet("{idPrecio:guid}/historial")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetHistorial(Guid idPrecio)
    {
        var historial = await service.ObtenerHistorialAsync(idPrecio);
        return Ok(historial);
    }
}
