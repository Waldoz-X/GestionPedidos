using System.Security.Claims;
using GestionPedidos.Contracts.Auth;
using GestionPedidos.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionPedidos.Controllers;

/// <summary>
/// Gestión de usuarios del sistema (empleados y clientes con acceso).
///
/// ┌──────────────────────────────────────────────────────────────────────────┐
/// │  POST   /api/usuarios/empleados/registrar        → Nuevo empleado+usuario│
/// │  POST   /api/usuarios/clientes/registrar         → Usuario para cliente  │
/// │  POST   /api/usuarios/empleados/{id}/acceso      → Acceso a empleado     │
/// │  POST   /api/usuarios/clientes/{id}/acceso       → Acceso a cliente      │
/// │  GET    /api/usuarios/{id}                       → Perfil de usuario     │
/// │  PUT    /api/usuarios/{id}/estado                → Activar/Inactivar     │
/// │  PUT    /api/usuarios/{id}/resetear-password     → Resetear contraseña   │
/// │  DELETE /api/usuarios/{id}                       → Eliminar usuario      │
/// └──────────────────────────────────────────────────────────────────────────┘
/// </summary>
[ApiController]
[Route("api/usuarios")]
[Authorize]
public class UsuariosController(IUsuarioService usuarioService) : ControllerBase
{
    // ── Listado ──────────────────────────────────────────────────────

    /// <summary>
    /// Obtiene todos los usuarios del sistema con su información completa:
    /// nombre, apellidos, departamento (empleados) o nombre comercial y moneda (clientes).
    /// Solo Admin y Manager.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ObtenerTodos()
    {
        var usuarios = await usuarioService.ObtenerTodosAsync();
        return Ok(usuarios);
    }

    // ── Registro ────────────────────────────────────────────────────

    /// <summary>
    /// Registra un nuevo empleado junto con su usuario de acceso al sistema.
    /// Crea el perfil de empleado y el usuario en un solo paso.
    /// Solo accesible para Admin y Manager.
    /// </summary>
    [HttpPost("empleados/registrar")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RegistrarEmpleado([FromBody] RegisterEmpleadoRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var resultado = await usuarioService.RegistrarEmpleadoAsync(request);

        if (!resultado.Success)
            return BadRequest(new { mensaje = resultado.Message, errores = resultado.Errors });

        return CreatedAtAction(
            nameof(ObtenerUsuario),
            new { id = resultado.Data!.IdUsuario },
            resultado);
    }

    /// <summary>
    /// Registra un usuario de acceso para un cliente ya existente en el sistema.
    /// El cliente debe existir previamente (creado desde ClientesController).
    /// Solo accesible para Admin y Manager.
    /// </summary>
    [HttpPost("clientes/registrar")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RegistrarClienteUser([FromBody] RegisterClienteUserRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var resultado = await usuarioService.RegistrarClienteUserAsync(request);

        if (!resultado.Success)
            return BadRequest(new { mensaje = resultado.Message, errores = resultado.Errors });

        return CreatedAtAction(
            nameof(ObtenerUsuario),
            new { id = resultado.Data!.IdUsuario },
            resultado);
    }

    // ── Creación de acceso para entidades existentes ─────────────────

    /// <summary>
    /// Crea credenciales de acceso para un empleado que ya existe pero no tiene usuario.
    /// Útil cuando el empleado fue dado de alta manualmente y después se le da acceso.
    /// Solo Admin.
    /// </summary>
    [HttpPost("empleados/{idEmpleado:guid}/acceso")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CrearAccesoEmpleado(Guid idEmpleado, [FromBody] CreateEmpleadoUserDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (dto.IdEmpleado != idEmpleado)
            return BadRequest(new { mensaje = "El ID del empleado en la URL no coincide con el del cuerpo." });

        var operador = ObtenerOperador();
        var resultado = await usuarioService.CrearUsuarioEmpleadoAsync(dto, operador);

        if (!resultado.Success)
            return BadRequest(new { mensaje = resultado.Message, errores = resultado.Errors });

        return CreatedAtAction(
            nameof(ObtenerUsuario),
            new { id = resultado.Data!.IdUsuario },
            resultado);
    }

    /// <summary>
    /// Crea credenciales de acceso para un cliente que ya existe pero no tiene usuario.
    /// Solo Admin.
    /// </summary>
    [HttpPost("clientes/{idCliente:guid}/acceso")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CrearAccesoCliente(Guid idCliente, [FromBody] CreateClienteUserDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (dto.IdCliente != idCliente)
            return BadRequest(new { mensaje = "El ID del cliente en la URL no coincide con el del cuerpo." });

        var operador = ObtenerOperador();
        var resultado = await usuarioService.CrearUsuarioClienteAsync(dto, operador);

        if (!resultado.Success)
            return BadRequest(new { mensaje = resultado.Message, errores = resultado.Errors });

        return CreatedAtAction(
            nameof(ObtenerUsuario),
            new { id = resultado.Data!.IdUsuario },
            resultado);
    }

    // ── Consulta ─────────────────────────────────────────────────────

    /// <summary>
    /// Obtiene el perfil completo de un usuario (datos de identidad + empleado o cliente).
    /// Admin y Manager pueden consultar cualquier usuario.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ObtenerUsuario(Guid id)
    {
        var usuario = await usuarioService.ObtenerUsuarioPorIdAsync(id);

        return usuario is null
            ? NotFound(new { mensaje = $"No se encontró usuario con ID {id}." })
            : Ok(usuario);
    }

    // ── Administración ────────────────────────────────────────────────

    /// <summary>
    /// Cambia el estado de un usuario: ACTIVO o INACTIVO.
    /// Un usuario INACTIVO no puede iniciar sesión.
    /// Solo Admin.
    /// </summary>
    [HttpPut("{id:guid}/estado")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] CambiarEstadoUsuarioRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var operador = ObtenerOperador();
        var cambiado = await usuarioService.CambiarEstadoUsuarioAsync(id, request.NuevoEstado.ToUpperInvariant(), operador);

        return cambiado
            ? Ok(new { mensaje = $"Estado del usuario actualizado a {request.NuevoEstado.ToUpperInvariant()}." })
            : NotFound(new { mensaje = $"No se encontró usuario con ID {id}." });
    }

    /// <summary>
    /// Resetea la contraseña de un usuario.
    /// El usuario deberá usar la nueva contraseña en su próximo inicio de sesión.
    /// Solo Admin.
    /// </summary>
    [HttpPut("{id:guid}/resetear-password")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResetearPassword(Guid id, [FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var operador = ObtenerOperador();
        var resultado = await usuarioService.ResetearPasswordAsync(id, request.NuevaPassword, operador);

        return resultado.Success
            ? Ok(new { mensaje = resultado.Message })
            : BadRequest(new { mensaje = resultado.Message, errores = resultado.Errors });
    }

    /// <summary>
    /// Elimina físicamente un usuario del sistema.
    /// Si tenía un empleado vinculado, se desvincula pero no se elimina el empleado.
    /// Solo Admin.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EliminarUsuario(Guid id)
    {
        var operador = ObtenerOperador();
        var eliminado = await usuarioService.EliminarUsuarioAsync(id, operador);

        return eliminado
            ? NoContent()
            : NotFound(new { mensaje = $"No se encontró usuario con ID {id}." });
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private string ObtenerOperador()
    {
        return User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? "SISTEMA";
    }
}
