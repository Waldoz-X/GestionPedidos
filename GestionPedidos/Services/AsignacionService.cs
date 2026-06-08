using GestionPedidos.Contracts.Empleados;
using GestionPedidos.Data;
using GestionPedidos.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPedidos.Services;

/// <summary>
/// Interfaz para gestionar asignaciones entre empleados y clientes
/// </summary>
public interface IAsignacionService
{
    /// <summary>
    /// Obtener todas las asignaciones con detalles del empleado y cliente
    /// </summary>
    Task<IEnumerable<AsignacionClienteEmpleadoDetalleDto>> ObtenerTodosAsync();

    /// <summary>
    /// Obtener asignación por empleado y cliente
    /// </summary>
    Task<AsignacionClienteEmpleadoDto?> ObtenerPorIdAsync(Guid idEmpleado, Guid idCliente);

    /// <summary>
    /// Crear una nueva asignación
    /// </summary>
    Task<AsignacionClienteEmpleadoDto> CrearAsync(AsignacionClienteEmpleadoCreateDto dto, string userEmail);

    /// <summary>
    /// Actualizar una asignación existente
    /// </summary>
    Task<AsignacionClienteEmpleadoDto?> ActualizarAsync(Guid idEmpleado, Guid idCliente, 
        AsignacionClienteEmpleadoUpdateDto dto, string userEmail);

    /// <summary>
    /// Eliminar una asignación
    /// </summary>
    Task<bool> EliminarAsync(Guid idEmpleado, Guid idCliente);

    /// <summary>
    /// Obtener todos los clientes de un empleado
    /// </summary>
    Task<IEnumerable<ClientesDelEmpleadoDto>> ObtenerClientesDelEmpleadoAsync(Guid idEmpleado);

    /// <summary>
    /// Obtener todos los empleados asignados a un cliente
    /// </summary>
    Task<IEnumerable<EmpleadosDelClienteDto>> ObtenerEmpleadosDelClienteAsync(Guid idCliente);

    /// <summary>
    /// Verificar si un empleado tiene acceso a un cliente
    /// </summary>
    Task<bool> EmpleadoTieneAccesoAClienteAsync(Guid idEmpleado, Guid idCliente);
}

public class AsignacionService(AppDbContext dbContext) : IAsignacionService
{
    public async Task<IEnumerable<AsignacionClienteEmpleadoDetalleDto>> ObtenerTodosAsync()
    {
        var asignaciones = await dbContext.AsignacionesClienteEmpleado
            .AsNoTracking()
            .Include(a => a.Empleado)
            .Include(a => a.Cliente)
            .ToListAsync();

        return asignaciones.Select(a => new AsignacionClienteEmpleadoDetalleDto(
            IdEmpleado: a.IdEmpleado,
            ClEmpleado: a.Empleado.ClEmpleado,
            NuEmpleado: a.Empleado.NuEmpleado,
            NbEmpleado: a.Empleado.NbEmpleado,
            NbApellidos: a.Empleado.NbApellidos,
            IdCliente: a.IdCliente,
            NbComercial: a.Cliente.NbComercial,
            ClTipoCliente: a.Cliente.ClTipoCliente,
            ClTipoRelacion: a.ClTipoRelacion,
            ClEstatusAsignacion: a.ClEstatusAsignacion,
            FeCreacion: a.FeCreacion
        )).ToList();
    }

    public async Task<AsignacionClienteEmpleadoDto?> ObtenerPorIdAsync(Guid idEmpleado, Guid idCliente)
    {
        var asignacion = await dbContext.AsignacionesClienteEmpleado
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.IdEmpleado == idEmpleado && a.IdCliente == idCliente);

        return asignacion == null ? null : MapToDto(asignacion);
    }

    public async Task<AsignacionClienteEmpleadoDto> CrearAsync(AsignacionClienteEmpleadoCreateDto dto, string userEmail)
    {
        // Validar que el empleado existe
        var empleado = await dbContext.Empleados.FirstOrDefaultAsync(e => e.IdEmpleado == dto.IdEmpleado);
        if (empleado == null)
            throw new InvalidOperationException($"Empleado con ID {dto.IdEmpleado} no existe");

        // Validar que el cliente existe
        var cliente = await dbContext.Clientes.FirstOrDefaultAsync(c => c.IdCliente == dto.IdCliente);
        if (cliente == null)
            throw new InvalidOperationException($"Cliente con ID {dto.IdCliente} no existe");

        // Validar que no existe una asignación anterior
        var existente = await dbContext.AsignacionesClienteEmpleado
            .FirstOrDefaultAsync(a => a.IdEmpleado == dto.IdEmpleado && a.IdCliente == dto.IdCliente);
        if (existente != null)
            throw new InvalidOperationException("Esta asignación ya existe");

        var asignacion = new etAsignacionClienteEmpleado
        {
            IdEmpleado = dto.IdEmpleado,
            IdCliente = dto.IdCliente,
            ClTipoRelacion = dto.ClTipoRelacion,
            ClEstatusAsignacion = "ACTIVO",
            ClOperadorCrea = userEmail,
            NbArtefactoCrea = "AsignacionService.CrearAsync",
            FeCreacion = DateTimeOffset.UtcNow
        };

        dbContext.AsignacionesClienteEmpleado.Add(asignacion);
        await dbContext.SaveChangesAsync();

        return MapToDto(asignacion);
    }

    public async Task<AsignacionClienteEmpleadoDto?> ActualizarAsync(Guid idEmpleado, Guid idCliente,
        AsignacionClienteEmpleadoUpdateDto dto, string userEmail)
    {
        var asignacion = await dbContext.AsignacionesClienteEmpleado
            .FirstOrDefaultAsync(a => a.IdEmpleado == idEmpleado && a.IdCliente == idCliente);

        if (asignacion == null) return null;

        asignacion.ClTipoRelacion = dto.ClTipoRelacion;
        asignacion.ClEstatusAsignacion = dto.ClEstatusAsignacion;
        asignacion.ClOperadorModifica = userEmail;
        asignacion.NbArtefactoModifica = "AsignacionService.ActualizarAsync";
        asignacion.FeModificacion = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync();
        return MapToDto(asignacion);
    }

    public async Task<bool> EliminarAsync(Guid idEmpleado, Guid idCliente)
    {
        var asignacion = await dbContext.AsignacionesClienteEmpleado
            .FirstOrDefaultAsync(a => a.IdEmpleado == idEmpleado && a.IdCliente == idCliente);

        if (asignacion == null) return false;

        dbContext.AsignacionesClienteEmpleado.Remove(asignacion);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ClientesDelEmpleadoDto>> ObtenerClientesDelEmpleadoAsync(Guid idEmpleado)
    {
        var clientes = await dbContext.AsignacionesClienteEmpleado
            .AsNoTracking()
            .Include(a => a.Cliente)
            .Where(a => a.IdEmpleado == idEmpleado && a.ClEstatusAsignacion == "ACTIVO")
            .Select(a => new ClientesDelEmpleadoDto(
                IdCliente: a.IdCliente,
                NbComercial: a.Cliente.NbComercial,
                ClTipoCliente: a.Cliente.ClTipoCliente,
                ClTipoRelacion: a.ClTipoRelacion,
                ClEstatusAsignacion: a.ClEstatusAsignacion
            ))
            .ToListAsync();

        return clientes;
    }

    public async Task<IEnumerable<EmpleadosDelClienteDto>> ObtenerEmpleadosDelClienteAsync(Guid idCliente)
    {
        var empleados = await dbContext.AsignacionesClienteEmpleado
            .AsNoTracking()
            .Include(a => a.Empleado)
            .Where(a => a.IdCliente == idCliente && a.ClEstatusAsignacion == "ACTIVO")
            .Select(a => new EmpleadosDelClienteDto(
                IdEmpleado: a.IdEmpleado,
                ClEmpleado: a.Empleado.ClEmpleado,
                NuEmpleado: a.Empleado.NuEmpleado,
                NbEmpleado: a.Empleado.NbEmpleado,
                NbApellidos: a.Empleado.NbApellidos,
                ClTipoRelacion: a.ClTipoRelacion,
                ClEstatusAsignacion: a.ClEstatusAsignacion
            ))
            .ToListAsync();

        return empleados;
    }

    public async Task<bool> EmpleadoTieneAccesoAClienteAsync(Guid idEmpleado, Guid idCliente)
    {
        return await dbContext.AsignacionesClienteEmpleado
            .AnyAsync(a => a.IdEmpleado == idEmpleado && 
                          a.IdCliente == idCliente && 
                          a.ClEstatusAsignacion == "ACTIVO");
    }

    private static AsignacionClienteEmpleadoDto MapToDto(etAsignacionClienteEmpleado a)
    {
        return new AsignacionClienteEmpleadoDto(
            a.IdEmpleado,
            a.IdCliente,
            a.ClTipoRelacion,
            a.ClEstatusAsignacion,
            a.FeCreacion,
            a.FeModificacion
        );
    }
}



