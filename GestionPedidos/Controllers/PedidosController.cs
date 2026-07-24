using GestionPedidos.Contracts.Pedidos;
using GestionPedidos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GestionPedidos.Controllers;

/// <summary>
/// Gestión de pedidos.
/// ── CLIENTE: crea borradores, agrega líneas, confirma.
/// ── EMPLEADO: gestiona pedidos de clientes asignados.
/// ── ADMIN: gestiona todos los pedidos, cambia estatus.
///
/// SEGURIDAD:
/// - Cada cliente solo ve y edita SUS pedidos (ownership via JWT).
/// - Los empleados solo ven pedidos de clientes asignados (via AsignacionClienteEmpleado).
/// - IdCliente NUNCA viene del body — siempre del JWT.
/// - Todas las mutaciones validan estatus del pedido.
/// </summary>
[ApiController]
[Route("api/pedidos")]
[Authorize]
public class PedidosController(IPedidoService service) : ControllerBase
{
    // ════════════════════════════════════════════════════════════════════════
    //  HELPERS DE SEGURIDAD
    // ════════════════════════════════════════════════════════════════════════

    private Guid ObtenerIdUsuarioDelJwt()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (claim == null || !Guid.TryParse(claim, out var idUsuario))
            throw new UnauthorizedAccessException("Token inválido: no se encontró el ID del usuario.");
        return idUsuario;
    }

    private Guid ObtenerIdClienteDelJwt()
    {
        var claim = User.FindFirst("idCliente")?.Value
                    ?? User.FindFirst(ClaimTypes.UserData)?.Value;
        if (claim == null || !Guid.TryParse(claim, out var idCliente))
            throw new UnauthorizedAccessException("El usuario no tiene un cliente asociado.");
        return idCliente;
    }

    private string ObtenerEmailDelJwt()
    {
        return User.Identity?.Name
            ?? User.FindFirst(ClaimTypes.Email)?.Value
            ?? "Sistema";
    }

    private bool EsAdmin()
    {
        return User.IsInRole("Admin");
    }

    private bool EsEmpleado()
    {
        return User.IsInRole("EMPLEADO") || User.IsInRole("Manager");
    }

    // ════════════════════════════════════════════════════════════════════════
    //  ENDPOINTS CLIENTE
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Crea un pedido en estado BORRADOR para el cliente logueado.
    /// El IdCliente se extrae del JWT — nunca del body.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "CLIENTE")]
    [Tags("Pedidos - Clientes")]
    public async Task<IActionResult> CrearPedido([FromBody] CrearPedidoRequest request)
    {
        try
        {
            var idCliente = ObtenerIdClienteDelJwt();
            var idUsuario = ObtenerIdUsuarioDelJwt();
            var email = ObtenerEmailDelJwt();

            var pedido = await service.CrearBorradorAsync(request, idCliente, idUsuario, email);
            return CreatedAtAction(nameof(ObtenerDetalle), new { idPedido = pedido.IdPedido }, pedido);
        }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>
    /// Lista los pedidos del cliente logueado. Solo ve los suyos.
    /// </summary>
    [HttpGet("mis-pedidos")]
    [Authorize(Roles = "CLIENTE")]
    [Tags("Pedidos - Clientes")]
    public async Task<IActionResult> MisPedidos()
    {
        try
        {
            var idCliente = ObtenerIdClienteDelJwt();
            var pedidos = await service.ObtenerPedidosClienteAsync(idCliente);
            return Ok(pedidos);
        }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
    }

    /// <summary>
    /// Agrega una línea al borrador.
    /// Valida visibilidad del producto, resuelve precio, verifica stock.
    /// </summary>
    [HttpPost("{idPedido:guid}/lineas")]
    [Authorize(Roles = "CLIENTE")]
    [Tags("Pedidos - Clientes")]
    public async Task<IActionResult> AgregarLinea(Guid idPedido, [FromBody] AgregarLineaRequest request)
    {
        try
        {
            var idCliente = ObtenerIdClienteDelJwt();
            var email = ObtenerEmailDelJwt();

            var linea = await service.AgregarLineaAsync(idPedido, request, idCliente, email);
            return Ok(linea);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>
    /// Actualiza la cantidad de una línea en el borrador.
    /// </summary>
    [HttpPut("{idPedido:guid}/lineas/{idLinea:guid}")]
    [Authorize(Roles = "CLIENTE")]
    [Tags("Pedidos - Clientes")]
    public async Task<IActionResult> ActualizarLinea(
        Guid idPedido, Guid idLinea, [FromBody] ActualizarLineaRequest request)
    {
        try
        {
            var idCliente = ObtenerIdClienteDelJwt();
            var email = ObtenerEmailDelJwt();

            var linea = await service.ActualizarLineaAsync(idPedido, idLinea, request, idCliente, email);
            return Ok(linea);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>
    /// Elimina una línea del borrador.
    /// </summary>
    [HttpDelete("{idPedido:guid}/lineas/{idLinea:guid}")]
    [Authorize(Roles = "CLIENTE")]
    [Tags("Pedidos - Clientes")]
    public async Task<IActionResult> EliminarLinea(Guid idPedido, Guid idLinea)
    {
        try
        {
            var idCliente = ObtenerIdClienteDelJwt();
            var eliminado = await service.EliminarLineaAsync(idPedido, idLinea, idCliente);
            if (!eliminado) return NotFound(new { message = "Línea no encontrada." });
            return NoContent();
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>
    /// Confirma el pedido: congela precios + reserva stock.
    /// Solo permitido en BORRADOR con ≥1 línea.
    /// </summary>
    [HttpPost("{idPedido:guid}/confirmar")]
    [Authorize(Roles = "CLIENTE")]
    [Tags("Pedidos - Clientes")]
    public async Task<IActionResult> ConfirmarPedido(Guid idPedido)
    {
        try
        {
            var idCliente = ObtenerIdClienteDelJwt();
            var idUsuario = ObtenerIdUsuarioDelJwt();
            var email = ObtenerEmailDelJwt();

            var pedido = await service.ConfirmarPedidoAsync(idPedido, idCliente, idUsuario, email);
            return Ok(pedido);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>
    /// El cliente cancela su propio borrador.
    /// </summary>
    [HttpPost("{idPedido:guid}/cancelar")]
    [Authorize(Roles = "CLIENTE")]
    [Tags("Pedidos - Clientes")]
    public async Task<IActionResult> CancelarBorrador(Guid idPedido)
    {
        try
        {
            var idCliente = ObtenerIdClienteDelJwt();
            var cancelado = await service.CancelarBorradorAsync(idPedido, idCliente);
            if (!cancelado) return NotFound(new { message = "Pedido no encontrado." });
            return Ok(new { message = "Borrador cancelado exitosamente." });
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  ENDPOINTS ADMIN / EMPLEADO ASIGNADO
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Cambia el estatus de un pedido (CONFIRMADO → FACTURADO → ENVIADO | CANCELADO).
    /// Acceso: Admin (todos) o Empleado asignado al cliente del pedido.
    /// </summary>
    [HttpPost("{idPedido:guid}/estatus")]
    [Authorize(Roles = "Admin,EMPLEADO,Manager")]
    [Tags("Pedidos - Administrativos / Empleados")]
    public async Task<IActionResult> CambiarEstatus(
        Guid idPedido, [FromBody] CambiarEstatusRequest request)
    {
        try
        {
            var idUsuario = ObtenerIdUsuarioDelJwt();
            var email = ObtenerEmailDelJwt();

            // ── SEGURIDAD: Empleados solo pueden gestionar pedidos de SUS clientes ──
            if (!EsAdmin())
            {
                var tieneAcceso = await service.EmpleadoTieneAccesoAlPedido(idUsuario, idPedido);
                if (!tieneAcceso)
                    return Forbid("No tienes acceso a este pedido. Solo puedes gestionar pedidos de tus clientes asignados.");
            }

            var pedido = await service.CambiarEstatusAsync(idPedido, request, idUsuario, email);
            return Ok(pedido);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>
    /// Lista todos los pedidos con filtros opcionales.
    /// Admin/Manager: ve todos los pedidos y puede filtrar por empleado asignado.
    /// Empleado: automáticamente se limita a ver pedidos de los clientes asignados en su cartera comercial.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,EMPLEADO,Manager")]
    [Tags("Pedidos - Administrativos / Empleados")]
    public async Task<IActionResult> ListarTodos(
        [FromQuery] string? estatus,
        [FromQuery] Guid? idCliente,
        [FromQuery] Guid? idEmpleado)
    {
        try
        {
            var idUsuario = ObtenerIdUsuarioDelJwt();
            var esAdmin = EsAdmin();

            var pedidos = await service.ObtenerTodosPedidosAsync(estatus, idCliente, idEmpleado, idUsuario, esAdmin);
            return Ok(pedidos);
        }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message }); }
    }

    /// <summary>
    /// Obtiene un resumen consolidado de pedidos y montos financieros para el tablero del Admin o Jefes de departamento.
    /// </summary>
    [HttpGet("dashboard-resumen")]
    [Authorize(Roles = "Admin,Manager")]
    [Tags("Pedidos - Admin Dashboard")]
    public async Task<IActionResult> ObtenerDashboard()
    {
        try
        {
            var resumen = await service.ObtenerDashboardResumenAsync();
            return Ok(resumen);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al consultar el dashboard resumen", error = ex.Message });
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  ENDPOINTS COMPARTIDOS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Obtiene el detalle completo de un pedido.
    /// ── CLIENTE: solo puede ver SUS pedidos.
    /// ── ADMIN: puede ver cualquiera.
    /// ── EMPLEADO: puede ver pedidos de sus clientes asignados.
    /// </summary>
    [HttpGet("{idPedido:guid}")]
    [Tags("Pedidos - Consultas Generales")]
    public async Task<IActionResult> ObtenerDetalle(Guid idPedido)
    {
        try
        {
            var pedido = await service.ObtenerPedidoDetalleAsync(idPedido);
            if (pedido == null)
                return NotFound(new { message = "Pedido no encontrado." });

            // ── SEGURIDAD: ownership check ──
            if (!EsAdmin())
            {
                if (EsEmpleado())
                {
                    var idUsuario = ObtenerIdUsuarioDelJwt();
                    var tieneAcceso = await service.EmpleadoTieneAccesoAlPedido(idUsuario, idPedido);
                    if (!tieneAcceso)
                        return Forbid("No tienes acceso a este pedido.");
                }
                else
                {
                    // Es CLIENTE — solo puede ver los suyos
                    var idCliente = ObtenerIdClienteDelJwt();
                    if (pedido.IdCliente != idCliente)
                        return Forbid("No tienes acceso a este pedido.");
                }
            }

            return Ok(pedido);
        }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
    }

    /// <summary>
    /// Endpoint temporal de prueba para forzar la expiración de un pedido en borrador.
    /// </summary>
    [HttpPost("{idPedido:guid}/expire-test")]
    [AllowAnonymous]
    [Tags("Pedidos - Pruebas")]
    public async Task<IActionResult> ExpireTest(Guid idPedido)
    {
        await service.ForzarExpiracionAsync(idPedido);
        return Ok(new { mensaje = "Fecha de expiración forzada al pasado (hace 5 minutos)." });
    }
}
