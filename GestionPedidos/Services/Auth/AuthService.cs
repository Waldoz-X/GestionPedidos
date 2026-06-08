using System;
using GestionPedidos.Contracts.Auth;
using GestionPedidos.Data;
using GestionPedidos.Models;
using GestionPedidos.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GestionPedidos.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<etUsuario> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IJwtTokenService _tokenService;
    private readonly AppDbContext _context;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<etUsuario> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IJwtTokenService tokenService,
        AppDbContext context,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _context = context;
        _logger = logger;
    }

    public async Task<AuthResponse> LoginAdminAsync(LoginRequest request)
    {
        try
        {
            _logger.LogInformation($"Login Admin iniciado para: {request.Email}");
            var usuario = await _userManager.FindByEmailAsync(request.Email);
            if (usuario == null)
            {
                return new AuthResponse(false, "Email o contraseña inválidos", null, new[] { "Usuario no encontrado" });
            }
            if (usuario.ClEstatusUsuario != "ACTIVO")
            {
                return new AuthResponse(false, "Usuario inactivo", null, new[] { "Este usuario ha sido desactivado" });
            }
            if (!await _userManager.CheckPasswordAsync(usuario, request.Password))
            {
                return new AuthResponse(false, "Email o contraseña inválidos", null, new[] { "Contraseña incorrecta" });
            }

            var roles = await _userManager.GetRolesAsync(usuario);
            var empleado = await _context.Set<Empleado>().FirstOrDefaultAsync(e => e.IdUsuario == usuario.Id);

            // Permitir si es empleado explícito o tiene roles administrativos
            if (empleado == null && !roles.Any(r => r == "Admin" || r == "Manager" || r == "User"))
            {
                return new AuthResponse(false, "Acceso denegado", null, new[] { "El usuario no tiene privilegios de administración" });
            }

            var token = _tokenService.GenerateToken(usuario, roles);
            var loginResponse = new LoginResponse
            {
                IdUsuario = usuario.Id,
                Email = usuario.Email ?? string.Empty,
                UserName = usuario.UserName ?? string.Empty,
                AccessToken = token,
                TipoUsuario = "EMPLEADO",
                IdEmpleado = empleado?.IdEmpleado,
                IdCliente = null,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                Roles = roles.ToList()
            };
            return new AuthResponse(true, "Login exitoso", loginResponse);
        }
        catch (Exception ex)
        {
            return new AuthResponse(false, "Error durante el login", null, new[] { ex.Message });
        }
    }

    public async Task<AuthResponse> LoginClientAsync(LoginRequest request)
    {
        try
        {
            _logger.LogInformation($"Login Client iniciado para: {request.Email}");
            var usuario = await _userManager.FindByEmailAsync(request.Email);
            if (usuario == null)
            {
                return new AuthResponse(false, "Email o contraseña inválidos", null, new[] { "Usuario no encontrado" });
            }
            if (usuario.ClEstatusUsuario != "ACTIVO")
            {
                return new AuthResponse(false, "Usuario inactivo", null, new[] { "Este usuario ha sido desactivado" });
            }
            if (!await _userManager.CheckPasswordAsync(usuario, request.Password))
            {
                return new AuthResponse(false, "Email o contraseña inválidos", null, new[] { "Contraseña incorrecta" });
            }

            var roles = await _userManager.GetRolesAsync(usuario);
            if (!roles.Contains("CLIENTE") && usuario.IdCliente == null)
            {
                return new AuthResponse(false, "Acceso denegado", null, new[] { "El usuario no es un cliente" });
            }

            var token = _tokenService.GenerateToken(usuario, roles);
            var loginResponse = new LoginResponse
            {
                IdUsuario = usuario.Id,
                Email = usuario.Email ?? string.Empty,
                UserName = usuario.UserName ?? string.Empty,
                AccessToken = token,
                TipoUsuario = "CLIENTE",
                IdEmpleado = null,
                IdCliente = usuario.IdCliente,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                Roles = roles.ToList()
            };
            return new AuthResponse(true, "Login exitoso", loginResponse);
        }
        catch (Exception ex)
        {
            return new AuthResponse(false, "Error durante el login", null, new[] { ex.Message });
        }
    }

    public async Task<AuthResponse> RegisterClienteAsync(RegisterClienteRequest request)
    {
        try
        {
            // Validar que las contraseñas coincidan
            if (request.Password != request.ConfirmPassword)
            {
                return new AuthResponse(false, "Los passwords no coinciden", null,
                    new[] { "Password y ConfirmPassword deben ser iguales" });
            }

            // Verificar si el email ya existe
            var usuarioExistente = await _userManager.FindByEmailAsync(request.Email);
            if (usuarioExistente != null)
            {
                return new AuthResponse(false, "El email ya está registrado", null,
                    new[] { "Este email ya tiene una cuenta" });
            }

            // Crear nuevo cliente
            var cliente = new etCliente
            {
                IdCliente = Guid.NewGuid(),
                NbComercial = request.NbComercial,
                ClTipoCliente = request.ClTipoCliente,
                IdElemMoneda = int.TryParse(request.ClMonedaIdCatalogo, out var monedaId) 
                    ? monedaId 
                    : throw new InvalidOperationException("MonedaId inválida"),
                MnLimiteCredito = 0,
                ClEstatusCliente = "ACTIVO",
                ClOperadorCrea = "REGISTRO_CLIENTE",
                NbArtefactoCrea = "SISTEMA"
            };

            // Crear usuario
            var usuario = new etUsuario
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                UserName = request.Email,
                EmailConfirmed = false, // En producción enviar email de confirmación
                ClEstatusUsuario = "ACTIVO",
                IdCliente = cliente.IdCliente,
                FeCreacion = DateTimeOffset.UtcNow
            };

            // Crear usuario con password
            var result = await _userManager.CreateAsync(usuario, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthResponse(false, "Error al crear usuario", null,
                    result.Errors.Select(e => e.Description).ToArray());
            }

            // Guardar cliente en BD
            _context.Add(cliente);
            await _context.SaveChangesAsync();

            // Asignar rol de CLIENTE
            await _userManager.AddToRoleAsync(usuario, "CLIENTE");

            // Generar token
            var token = _tokenService.GenerateToken(usuario, new[] { "CLIENTE" });
            var expiresAt = DateTime.UtcNow.AddMinutes(60);

            var loginResponse = new LoginResponse
            {
                IdUsuario = usuario.Id,
                Email = usuario.Email ?? string.Empty,
                UserName = usuario.UserName ?? string.Empty,
                AccessToken = token,
                TipoUsuario = "CLIENTE",
                IdEmpleado = null,
                IdCliente = cliente.IdCliente,
                ExpiresAt = expiresAt,
                Roles = new[] { "CLIENTE" }
            };

            return new AuthResponse(true, "Registro exitoso", loginResponse);
        }
        catch (Exception ex)
        {
            return new AuthResponse(false, "Error durante el registro", null,
                new[] { ex.Message });
        }
    }

    public async Task<AuthResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        try
        {
            // Validar que las contraseñas coincidan
            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return new AuthResponse(false, "Los passwords no coinciden", null,
                    new[] { "NewPassword y ConfirmNewPassword deben ser iguales" });
            }

            // Obtener usuario
            var usuario = await _userManager.FindByIdAsync(userId.ToString());
            if (usuario == null)
            {
                return new AuthResponse(false, "Usuario no encontrado", null,
                    new[] { "El usuario especificado no existe" });
            }

            // Cambiar password
            var result = await _userManager.ChangePasswordAsync(
                usuario, 
                request.CurrentPassword, 
                request.NewPassword);

            if (!result.Succeeded)
            {
                return new AuthResponse(false, "Error al cambiar la contraseña", null,
                    result.Errors.Select(e => e.Description).ToArray());
            }

            return new AuthResponse(true, "Contraseña cambiada exitosamente");
        }
        catch (Exception ex)
        {
            return new AuthResponse(false, "Error durante el cambio de contraseña", null,
                new[] { ex.Message });
        }
    }

    public async Task<UserProfileResponse?> GetUserProfileAsync(Guid userId)
    {
        try
        {
            var usuario = await _context.Set<etUsuario>()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (usuario == null)
                return null;

            var roles = await _userManager.GetRolesAsync(usuario);

            // Obtener datos de empleado si existe
            EmpleadoProfileDto? empleadoDto = null;
            var empleado = await _context.Set<Empleado>()
                .Include(e => e.Area)
                .FirstOrDefaultAsync(e => e.IdUsuario == usuario.Id);

            if (empleado != null)
            {
                empleadoDto = new EmpleadoProfileDto(
                    IdEmpleado: empleado.IdEmpleado,
                    ClEmpleado: empleado.ClEmpleado,
                    NbEmpleado: empleado.NbEmpleado,
                    NbApellidos: empleado.NbApellidos,
                    Area: empleado.Area?.NbCatalogoElemento ?? "Sin área"
                );
            }

            // Obtener datos de cliente si existe
            ClienteProfileDto? clienteDto = null;
            if (usuario.IdCliente.HasValue)
            {
                var cliente = await _context.Set<etCliente>()
                    .Include(c => c.ClMoneda)
                    .FirstOrDefaultAsync(c => c.IdCliente == usuario.IdCliente);

                if (cliente != null)
                {
                    clienteDto = new ClienteProfileDto(
                        IdCliente: cliente.IdCliente,
                        NbComercial: cliente.NbComercial,
                        ClTipoCliente: cliente.ClTipoCliente,
                        MnLimiteCredito: cliente.MnLimiteCredito,
                        Moneda: cliente.ClMoneda?.NbCatalogoElemento ?? "USD"
                    );
                }
            }

            var tipoUsuario = empleadoDto != null ? "EMPLEADO" : "CLIENTE";

            return new UserProfileResponse(
                IdUsuario: usuario.Id,
                Email: usuario.Email ?? string.Empty,
                UserName: usuario.UserName ?? string.Empty,
                TipoUsuario: tipoUsuario,
                Empleado: empleadoDto,
                Cliente: clienteDto,
                Roles: roles.ToArray()
            );
        }
        catch
        {
            return null;
        }
    }

    public async Task<etUsuario?> GetUserByIdAsync(Guid userId)
    {
        return await _userManager.FindByIdAsync(userId.ToString());
    }

    public async Task LogoutAsync(Guid userId)
    {
        // Con JWT stateless no se necesita hacer nada en servidor
        // El cliente simplemente descarta el token
        // Si implementaras token blacklist, aquí lo haría
        await Task.CompletedTask;
    }
}




