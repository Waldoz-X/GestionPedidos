using GestionPedidos.Contracts.Empleados;
using GestionPedidos.Data;
using GestionPedidos.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPedidos.Services;

public interface IEmpleadoService
{
    Task<IEnumerable<EmpleadoDto>> ObtenerTodosAsync();
    Task<EmpleadoDto?> ObtenerPorIdAsync(Guid idEmpleado);
    Task<EmpleadoDto> CrearAsync(EmpleadoCreateDto dto, string userEmail);
    Task<EmpleadoDto?> ActualizarAsync(Guid idEmpleado, EmpleadoUpdateDto dto, string userEmail);
    Task<bool> EliminarAsync(Guid idEmpleado);
}

public class EmpleadoService(AppDbContext dbContext) : IEmpleadoService
{
    public async Task<IEnumerable<EmpleadoDto>> ObtenerTodosAsync()
    {
        var empleados = await dbContext.Empleados
            .Include(e => e.Usuario)
            .AsNoTracking()
            .ToListAsync();

        return empleados.Select(MapToDto);
    }

    public async Task<EmpleadoDto?> ObtenerPorIdAsync(Guid idEmpleado)
    {
        var empleado = await dbContext.Empleados
            .Include(e => e.Usuario)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.IdEmpleado == idEmpleado);

        return empleado == null ? null : MapToDto(empleado);
    }

    public async Task<EmpleadoDto> CrearAsync(EmpleadoCreateDto dto, string userEmail)
    {
        var empleado = new etEmpleado
        {
            IdEmpleado = Guid.NewGuid(),
            IdUsuario = dto.IdUsuario,
            IdElemArea = dto.IdElemArea,
            NuEmpleado = dto.NuEmpleado,
            ClEmpleado = dto.ClEmpleado,
            NbEmpleado = dto.NbEmpleado,
            NbApellidos = dto.NbApellidos,
            ClEstatusEmpleado = dto.ClEstatusEmpleado,
            ClOperadorCrea = userEmail,
            NbArtefactoCrea = "EmpleadoService.CrearAsync"
        };

        dbContext.Empleados.Add(empleado);
        await dbContext.SaveChangesAsync();

        return MapToDto(empleado);
    }

    public async Task<EmpleadoDto?> ActualizarAsync(Guid idEmpleado, EmpleadoUpdateDto dto, string userEmail)
    {
        var empleado = await dbContext.Empleados
            .Include(e => e.Usuario)
            .FirstOrDefaultAsync(e => e.IdEmpleado == idEmpleado);

        if (empleado == null) return null;

        empleado.IdUsuario = dto.IdUsuario;
        empleado.IdElemArea = dto.IdElemArea;
        empleado.NuEmpleado = dto.NuEmpleado;
        empleado.ClEmpleado = dto.ClEmpleado;
        empleado.NbEmpleado = dto.NbEmpleado;
        empleado.NbApellidos = dto.NbApellidos;
        empleado.ClEstatusEmpleado = dto.ClEstatusEmpleado;
        
        empleado.ClOperadorModifica = userEmail;
        empleado.NbArtefactoModifica = "EmpleadoService.ActualizarAsync";
        empleado.FeModificacion = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync();
        return MapToDto(empleado);
    }

    public async Task<bool> EliminarAsync(Guid idEmpleado)
    {
        var empleado = await dbContext.Empleados
            .FirstOrDefaultAsync(e => e.IdEmpleado == idEmpleado);

        if (empleado == null) return false;

        dbContext.Empleados.Remove(empleado);
        await dbContext.SaveChangesAsync();
        return true;
    }

    private static EmpleadoDto MapToDto(etEmpleado e)
    {
        return new EmpleadoDto(
            e.IdEmpleado,
            e.IdUsuario,
            e.IdElemArea,
            e.NuEmpleado,
            e.ClEmpleado,
            e.NbEmpleado,
            e.NbApellidos,
            e.ClEstatusEmpleado,
            e.Usuario?.Email,
            e.FeCreacion,
            e.FeModificacion
        );
    }
}
