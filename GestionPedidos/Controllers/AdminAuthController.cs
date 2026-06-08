using System.Security.Claims;
using GestionPedidos.Contracts.Auth;
using GestionPedidos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionPedidos.Controllers;

/// <summary>
/// Autenticación para empleados, gerentes y administradores.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AdminAuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AdminAuthController> _logger;

    public AdminAuthController(IAuthService authService, ILogger<AdminAuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _logger.LogInformation($"Intento de login admin para: {request.Email}");

        var response = await _authService.LoginAdminAsync(request);

        if (!response.Success)
        {
            _logger.LogWarning($"Login admin fallido para: {request.Email}. Razón: {response.Message}");
            return Unauthorized(response);
        }

        _logger.LogInformation($"Login admin exitoso para: {request.Email}");
        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Admin,Manager,User,EMPLEADO")]
    public async Task<ActionResult<UserProfileResponse>> GetProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Token inválido" });

        var profile = await _authService.GetUserProfileAsync(userId);
        if (profile == null)
            return NotFound(new { message = "Usuario no encontrado" });

        return Ok(profile);
    }

    [HttpPost("change-password")]
    [Authorize(Roles = "Admin,Manager,User,EMPLEADO")]
    public async Task<ActionResult<AuthResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Token inválido" });

        var response = await _authService.ChangePasswordAsync(userId, request);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize(Roles = "Admin,Manager,User,EMPLEADO")]
    public async Task<ActionResult<AuthResponse>> Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Token inválido" });

        await _authService.LogoutAsync(userId);
        return Ok(new AuthResponse(true, "Logout exitoso"));
    }
}
