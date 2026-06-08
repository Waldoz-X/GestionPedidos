using GestionPedidos.Contracts.Auth;
using GestionPedidos.Data;
using GestionPedidos.Models;
using GestionPedidos.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GestionPedidos.Services.Auth;

/// <summary>
/// Interfaz para gestionar usuarios (crear accesos a empleados y clientes)
/// </summary>
public interface IUsuarioService
{
    /// <summary>
    /// Obtener todos los usuarios con su información completa
    /// </summary>
    Task<IReadOnlyList<UsuarioListDto>> ObtenerTodosAsync();

    /// <summary>
    /// Registrar nuevo empleado con usuario
    /// </summary>
    Task<CreateUserResponse> RegistrarEmpleadoAsync(RegisterEmpleadoRequest request);

    /// <summary>
    /// Registrar nuevo usuario para cliente existente
    /// </summary>
    Task<CreateUserResponse> RegistrarClienteUserAsync(RegisterClienteUserRequest request);

    /// <summary>
    /// Crear usuario para empleado existente (Admin)
    /// </summary>
    Task<CreateUserResponse> CrearUsuarioEmpleadoAsync(CreateEmpleadoUserDto dto, string userEmail);

    /// <summary>
    /// Crear usuario para cliente existente (Admin)
    /// </summary>
    Task<CreateUserResponse> CrearUsuarioClienteAsync(CreateClienteUserDto dto, string userEmail);

    /// <summary>
    /// Cambiar estado de usuario
    /// </summary>
    Task<bool> CambiarEstadoUsuarioAsync(Guid idUsuario, string nuevoEstado, string userEmail);

    /// <summary>
    /// Resetear contraseña de usuario
    /// </summary>
    Task<CreateUserResponse> ResetearPasswordAsync(Guid idUsuario, string newPassword, string userEmail);

    /// <summary>
    /// Obtener usuario por ID
    /// </summary>
    Task<UserProfileResponse?> ObtenerUsuarioPorIdAsync(Guid idUsuario);

    /// <summary>
    /// Eliminar usuario
    /// </summary>
    Task<bool> EliminarUsuarioAsync(Guid idUsuario, string userEmail);
}

public class UsuarioService : IUsuarioService
{
    private readonly UserManager<etUsuario> _userManager;
    private readonly AppDbContext _context;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(
        UserManager<etUsuario> userManager,
        AppDbContext context,
        ILogger<UsuarioService> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UsuarioListDto>> ObtenerTodosAsync()
    {
        // ── Traer usuarios, empleados, clientes y roles en paralelo ──
        var usuarios  = await _context.Users.AsNoTracking().ToListAsync();
        var empleados = await _context.Empleados
            .AsNoTracking()
            .Include(e => e.Area)
            .ToListAsync();
        var clientes  = await _context.Clientes
            .AsNoTracking()
            .Include(c => c.ClMoneda)
            .ToListAsync();

        // Roles: join UserRoles ↔ Roles en memoria para evitar N+1
        var userRoles = await _context.UserRoles.AsNoTracking().ToListAsync();
        var rolesCat  = await _context.Roles.AsNoTracking().ToListAsync();

        var empleadosPorUsuario = empleados
            .Where(e => e.IdUsuario.HasValue)
            .ToDictionary(e => e.IdUsuario!.Value);

        var clientesPorId = clientes.ToDictionary(c => c.IdCliente);

        var rolesPorUsuario = userRoles
            .GroupBy(ur => ur.UserId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(ur => rolesCat.FirstOrDefault(r => r.Id == ur.RoleId)?.Name ?? "")
                       .Where(n => n.Length > 0)
                       .ToArray());

        return usuarios.Select(u =>
        {
            var roles = rolesPorUsuario.TryGetValue(u.Id, out var r) ? r : Array.Empty<string>();

            UsuarioEmpleadoDto? empleadoDto = null;
            if (empleadosPorUsuario.TryGetValue(u.Id, out var emp))
            {
                empleadoDto = new UsuarioEmpleadoDto(
                    IdEmpleado:       emp.IdEmpleado,
                    ClEmpleado:       emp.ClEmpleado,
                    NbEmpleado:       emp.NbEmpleado,
                    NbApellidos:      emp.NbApellidos,
                    NbNombreCompleto: $"{emp.NbEmpleado} {emp.NbApellidos}",
                    NbDepartamento:   emp.Area?.NbCatalogoElemento,
                    ClEstatusEmpleado: emp.ClEstatusEmpleado
                );
            }

            UsuarioClienteDto? clienteDto = null;
            if (u.IdCliente.HasValue && clientesPorId.TryGetValue(u.IdCliente.Value, out var cli))
            {
                clienteDto = new UsuarioClienteDto(
                    IdCliente:       cli.IdCliente,
                    NbComercial:     cli.NbComercial,
                    ClTipoCliente:   cli.ClTipoCliente,
                    MnLimiteCredito: cli.MnLimiteCredito,
                    NbMoneda:        cli.ClMoneda?.NbCatalogoElemento ?? "USD",
                    ClEstatusCliente: cli.ClEstatusCliente
                );
            }

            var tipoUsuario = empleadoDto is not null ? "EMPLEADO"
                            : clienteDto  is not null ? "CLIENTE"
                            : "ADMIN";

            return new UsuarioListDto(
                IdUsuario:        u.Id,
                Email:            u.Email ?? string.Empty,
                TipoUsuario:      tipoUsuario,
                ClEstatusUsuario: u.ClEstatusUsuario,
                Roles:            roles,
                FeCreacion:       u.FeCreacion,
                Empleado:         empleadoDto,
                Cliente:          clienteDto
            );
        })
        .OrderBy(u => u.TipoUsuario)
        .ThenBy(u => u.Email)
        .ToList();
    }

    public async Task<CreateUserResponse> RegistrarEmpleadoAsync(RegisterEmpleadoRequest request)
    {
        try
        {
            // ── Validaciones ──
            if (request.Password != request.ConfirmPassword)
                return new CreateUserResponse(false, "Las contraseñas no coinciden", null,
                    new[] { "Password y ConfirmPassword deben ser iguales" });

            var usuarioExistente = await _userManager.FindByEmailAsync(request.Email);
            if (usuarioExistente != null)
                return new CreateUserResponse(false, "El email ya está registrado", null,
                    new[] { "Este email ya tiene una cuenta" });

            // ── Crear empleado ──
            var empleado = new etEmpleado
            {
                IdEmpleado = Guid.NewGuid(),
                ClEmpleado = request.ClEmpleado,
                NbEmpleado = request.NbEmpleado,
                NbApellidos = request.NbApellidos,
                IdElemArea = request.IdElemArea,
                ClEstatusEmpleado = "ACTIVO",
                ClOperadorCrea = "REGISTRO_EMPLEADO",
                NbArtefactoCrea = "SISTEMA",
                FeCreacion = DateTimeOffset.UtcNow
            };

            // ── Crear usuario ──
            var usuario = new etUsuario
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                UserName = request.Email,
                EmailConfirmed = false,
                ClEstatusUsuario = "ACTIVO",
                FeCreacion = DateTimeOffset.UtcNow
            };

            // ── Crear usuario con contraseña ──
            var result = await _userManager.CreateAsync(usuario, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                _logger.LogWarning($"Error al crear usuario empleado: {string.Join(", ", errors)}");
                return new CreateUserResponse(false, "Error al crear usuario", null, errors);
            }

            // ── Asociar empleado con usuario ──
            empleado.IdUsuario = usuario.Id;
            _context.Empleados.Add(empleado);
            await _context.SaveChangesAsync();

            // ── Asignar rol EMPLEADO ──
            await _userManager.AddToRoleAsync(usuario, "EMPLEADO");

            _logger.LogInformation($"Empleado registrado exitosamente: {request.Email}");

            return new CreateUserResponse(
                true,
                "Empleado registrado exitosamente",
                new CreatedUserData(
                    IdUsuario: usuario.Id,
                    Email: usuario.Email,
                    UserName: usuario.UserName,
                    TipoUsuario: "EMPLEADO",
                    Roles: new[] { "EMPLEADO" }
                )
            );
        }
        catch (Exception ex)
        {
            _logger.LogError($"Excepción al registrar empleado: {ex.Message}");
            return new CreateUserResponse(false, "Error durante el registro", null,
                new[] { ex.Message });
        }
    }

    public async Task<CreateUserResponse> RegistrarClienteUserAsync(RegisterClienteUserRequest request)
    {
        try
        {
            // ── Validaciones ──
            if (request.Password != request.ConfirmPassword)
                return new CreateUserResponse(false, "Las contraseñas no coinciden", null,
                    new[] { "Password y ConfirmPassword deben ser iguales" });

            var usuarioExistente = await _userManager.FindByEmailAsync(request.Email);
            if (usuarioExistente != null)
                return new CreateUserResponse(false, "El email ya está registrado", null,
                    new[] { "Este email ya tiene una cuenta" });

            // ── Verificar que el cliente existe ──
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdCliente == request.IdCliente);
            if (cliente == null)
                return new CreateUserResponse(false, "Cliente no encontrado", null,
                    new[] { $"No existe cliente con ID {request.IdCliente}" });

            // ── Crear usuario ──
            var usuario = new etUsuario
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                UserName = request.Email,
                EmailConfirmed = false,
                ClEstatusUsuario = "ACTIVO",
                IdCliente = request.IdCliente,
                FeCreacion = DateTimeOffset.UtcNow
            };

            // ── Crear usuario con contraseña ──
            var result = await _userManager.CreateAsync(usuario, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                _logger.LogWarning($"Error al crear usuario cliente: {string.Join(", ", errors)}");
                return new CreateUserResponse(false, "Error al crear usuario", null, errors);
            }

            // ── Asignar rol CLIENTE ──
            await _userManager.AddToRoleAsync(usuario, "CLIENTE");

            _logger.LogInformation($"Usuario cliente registrado exitosamente: {request.Email}");

            return new CreateUserResponse(
                true,
                "Usuario cliente registrado exitosamente",
                new CreatedUserData(
                    IdUsuario: usuario.Id,
                    Email: usuario.Email,
                    UserName: usuario.UserName,
                    TipoUsuario: "CLIENTE",
                    Roles: new[] { "CLIENTE" }
                )
            );
        }
        catch (Exception ex)
        {
            _logger.LogError($"Excepción al registrar usuario cliente: {ex.Message}");
            return new CreateUserResponse(false, "Error durante el registro", null,
                new[] { ex.Message });
        }
    }

    public async Task<CreateUserResponse> CrearUsuarioEmpleadoAsync(CreateEmpleadoUserDto dto, string userEmail)
    {
        try
        {
            // ── Validar que el email no exista ──
            var usuarioExistente = await _userManager.FindByEmailAsync(dto.Email);
            if (usuarioExistente != null)
                return new CreateUserResponse(false, "El email ya está registrado", null,
                    new[] { "Este email ya tiene una cuenta" });

            // ── Verificar que el empleado existe ──
            var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.IdEmpleado == dto.IdEmpleado);
            if (empleado == null)
                return new CreateUserResponse(false, "Empleado no encontrado", null,
                    new[] { $"No existe empleado con ID {dto.IdEmpleado}" });

            // ── Verificar que el empleado no tiene usuario asignado ──
            if (empleado.IdUsuario.HasValue)
                return new CreateUserResponse(false, "El empleado ya tiene usuario asignado", null,
                    new[] { "Este empleado ya tiene acceso al sistema" });

            // ── Crear usuario ──
            var usuario = new etUsuario
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                UserName = dto.Email,
                EmailConfirmed = false,
                ClEstatusUsuario = "ACTIVO",
                FeCreacion = DateTimeOffset.UtcNow
            };

            // ── Crear usuario con contraseña ──
            var result = await _userManager.CreateAsync(usuario, dto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                _logger.LogWarning($"Error al crear usuario para empleado: {string.Join(", ", errors)}");
                return new CreateUserResponse(false, "Error al crear usuario", null, errors);
            }

            // ── Asociar usuario con empleado ──
            empleado.IdUsuario = usuario.Id;
            await _context.SaveChangesAsync();

            // ── Asignar rol EMPLEADO ──
            await _userManager.AddToRoleAsync(usuario, "EMPLEADO");

            _logger.LogInformation($"Usuario creado para empleado {dto.IdEmpleado}: {dto.Email} (por {userEmail})");

            return new CreateUserResponse(
                true,
                "Usuario creado exitosamente para empleado",
                new CreatedUserData(
                    IdUsuario: usuario.Id,
                    Email: usuario.Email,
                    UserName: usuario.UserName,
                    TipoUsuario: "EMPLEADO",
                    Roles: new[] { "EMPLEADO" }
                )
            );
        }
        catch (Exception ex)
        {
            _logger.LogError($"Excepción al crear usuario para empleado: {ex.Message}");
            return new CreateUserResponse(false, "Error durante la creación del usuario", null,
                new[] { ex.Message });
        }
    }

    public async Task<CreateUserResponse> CrearUsuarioClienteAsync(CreateClienteUserDto dto, string userEmail)
    {
        try
        {
            // ── Validar que el email no exista ──
            var usuarioExistente = await _userManager.FindByEmailAsync(dto.Email);
            if (usuarioExistente != null)
                return new CreateUserResponse(false, "El email ya está registrado", null,
                    new[] { "Este email ya tiene una cuenta" });

            // ── Verificar que el cliente existe ──
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdCliente == dto.IdCliente);
            if (cliente == null)
                return new CreateUserResponse(false, "Cliente no encontrado", null,
                    new[] { $"No existe cliente con ID {dto.IdCliente}" });

            // ── Crear usuario ──
            var usuario = new etUsuario
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                UserName = dto.Email,
                EmailConfirmed = false,
                ClEstatusUsuario = "ACTIVO",
                IdCliente = dto.IdCliente,
                FeCreacion = DateTimeOffset.UtcNow
            };

            // ── Crear usuario con contraseña ──
            var result = await _userManager.CreateAsync(usuario, dto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                _logger.LogWarning($"Error al crear usuario para cliente: {string.Join(", ", errors)}");
                return new CreateUserResponse(false, "Error al crear usuario", null, errors);
            }

            // ── Asignar rol CLIENTE ──
            await _userManager.AddToRoleAsync(usuario, "CLIENTE");

            _logger.LogInformation($"Usuario creado para cliente {dto.IdCliente}: {dto.Email} (por {userEmail})");

            return new CreateUserResponse(
                true,
                "Usuario creado exitosamente para cliente",
                new CreatedUserData(
                    IdUsuario: usuario.Id,
                    Email: usuario.Email,
                    UserName: usuario.UserName,
                    TipoUsuario: "CLIENTE",
                    Roles: new[] { "CLIENTE" }
                )
            );
        }
        catch (Exception ex)
        {
            _logger.LogError($"Excepción al crear usuario para cliente: {ex.Message}");
            return new CreateUserResponse(false, "Error durante la creación del usuario", null,
                new[] { ex.Message });
        }
    }

    public async Task<bool> CambiarEstadoUsuarioAsync(Guid idUsuario, string nuevoEstado, string userEmail)
    {
        try
        {
            var usuario = await _userManager.FindByIdAsync(idUsuario.ToString());
            if (usuario == null) return false;

            if (nuevoEstado != "ACTIVO" && nuevoEstado != "INACTIVO")
                throw new InvalidOperationException("Estado inválido");

            usuario.ClEstatusUsuario = nuevoEstado;
            usuario.FeModificacion = DateTimeOffset.UtcNow;

            var result = await _userManager.UpdateAsync(usuario);
            if (result.Succeeded)
            {
                _logger.LogInformation($"Estado del usuario {idUsuario} cambiado a {nuevoEstado} (por {userEmail})");
            }

            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al cambiar estado de usuario: {ex.Message}");
            return false;
        }
    }

    public async Task<CreateUserResponse> ResetearPasswordAsync(Guid idUsuario, string newPassword, string userEmail)
    {
        try
        {
            var usuario = await _userManager.FindByIdAsync(idUsuario.ToString());
            if (usuario == null)
                return new CreateUserResponse(false, "Usuario no encontrado", null,
                    new[] { "El usuario especificado no existe" });

            // ── Remover contraseña anterior ──
            var removeResult = await _userManager.RemovePasswordAsync(usuario);
            if (!removeResult.Succeeded)
                return new CreateUserResponse(false, "Error al resetear contraseña", null,
                    removeResult.Errors.Select(e => e.Description).ToArray());

            // ── Agregar nueva contraseña ──
            var addResult = await _userManager.AddPasswordAsync(usuario, newPassword);
            if (!addResult.Succeeded)
                return new CreateUserResponse(false, "Error al establecer nueva contraseña", null,
                    addResult.Errors.Select(e => e.Description).ToArray());

            _logger.LogInformation($"Contraseña reseteada para usuario {idUsuario} (por {userEmail})");

            return new CreateUserResponse(true, "Contraseña reseteada exitosamente", null);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Excepción al resetear password: {ex.Message}");
            return new CreateUserResponse(false, "Error durante el reseteo de contraseña", null,
                new[] { ex.Message });
        }
    }

    public async Task<UserProfileResponse?> ObtenerUsuarioPorIdAsync(Guid idUsuario)
    {
        try
        {
            var usuario = await _context.Set<etUsuario>()
                .FirstOrDefaultAsync(u => u.Id == idUsuario);

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

    public async Task<bool> EliminarUsuarioAsync(Guid idUsuario, string userEmail)
    {
        try
        {
            var usuario = await _userManager.FindByIdAsync(idUsuario.ToString());
            if (usuario == null) return false;

            // Si es empleado, desasociar
            var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.IdUsuario == idUsuario);
            if (empleado != null)
            {
                empleado.IdUsuario = null;
                await _context.SaveChangesAsync();
            }

            var result = await _userManager.DeleteAsync(usuario);
            if (result.Succeeded)
            {
                _logger.LogInformation($"Usuario {idUsuario} eliminado (por {userEmail})");
            }

            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al eliminar usuario: {ex.Message}");
            return false;
        }
    }
}

