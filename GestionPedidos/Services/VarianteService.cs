using GestionPedidos.Contracts.Variantes;
using GestionPedidos.Data;
using GestionPedidos.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPedidos.Services;

public interface IVarianteService
{
    Task<IEnumerable<VarianteDto>> ObtenerTodosAsync();
    Task<IEnumerable<VarianteDto>> ObtenerPorProductoAsync(Guid idProducto);
    Task<VarianteDto?> ObtenerPorIdAsync(Guid idVariante);
    Task<VarianteDto> CrearAsync(VarianteCreateDto dto, string userEmail);
    Task<VarianteDto?> ActualizarAsync(Guid idVariante, VarianteUpdateDto dto, string userEmail);
    Task<bool> EliminarAsync(Guid idVariante);
}

public class VarianteService(AppDbContext dbContext) : IVarianteService
{
    public async Task<IEnumerable<VarianteDto>> ObtenerTodosAsync()
    {
        var variantes = await dbContext.Variantes
            .AsNoTracking()
            .ToListAsync();

        return variantes.Select(MapToDto);
    }

    public async Task<IEnumerable<VarianteDto>> ObtenerPorProductoAsync(Guid idProducto)
    {
        var variantes = await dbContext.Variantes
            .Where(v => v.IdProducto == idProducto)
            .AsNoTracking()
            .ToListAsync();

        return variantes.Select(MapToDto);
    }

    public async Task<VarianteDto?> ObtenerPorIdAsync(Guid idVariante)
    {
        var variante = await dbContext.Variantes
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.IdVariante == idVariante);

        return variante == null ? null : MapToDto(variante);
    }

    public async Task<VarianteDto> CrearAsync(VarianteCreateDto dto, string userEmail)
    {
        var variante = new etVariante
        {
            IdVariante = Guid.NewGuid(),
            IdProducto = dto.IdProducto,
            IdElemCombinacion = dto.IdElemCombinacion,
            UrlImagen = dto.UrlImagen,
            ClEstatusVariante = dto.ClEstatusVariante,
            ClOperadorCrea = userEmail,
            NbArtefactoCrea = "VarianteService.CrearAsync"
        };

        dbContext.Variantes.Add(variante);
        await dbContext.SaveChangesAsync();

        return MapToDto(variante);
    }

    public async Task<VarianteDto?> ActualizarAsync(Guid idVariante, VarianteUpdateDto dto, string userEmail)
    {
        var variante = await dbContext.Variantes
            .FirstOrDefaultAsync(v => v.IdVariante == idVariante);

        if (variante == null) return null;

        variante.IdElemCombinacion = dto.IdElemCombinacion;
        variante.UrlImagen = dto.UrlImagen;
        variante.ClEstatusVariante = dto.ClEstatusVariante;

        variante.ClOperadorModifica = userEmail;
        variante.NbArtefactoModifica = "VarianteService.ActualizarAsync";
        variante.FeModificacion = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync();
        return MapToDto(variante);
    }

    public async Task<bool> EliminarAsync(Guid idVariante)
    {
        var variante = await dbContext.Variantes
            .FirstOrDefaultAsync(v => v.IdVariante == idVariante);

        if (variante == null) return false;

        dbContext.Variantes.Remove(variante);
        await dbContext.SaveChangesAsync();
        return true;
    }

    private static VarianteDto MapToDto(etVariante v)
    {
        return new VarianteDto(
            v.IdVariante,
            v.IdProducto,
            v.IdElemCombinacion,
            v.UrlImagen,
            v.ClEstatusVariante,
            v.FeCreacion,
            v.FeModificacion
        );
    }
}
