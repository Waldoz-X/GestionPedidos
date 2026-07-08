using GestionPedidos.Contracts.Skus;
using GestionPedidos.Data;
using GestionPedidos.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPedidos.Services;

public interface ISkuService
{
    Task<IEnumerable<SkuDto>> ObtenerTodosAsync();
    Task<IEnumerable<SkuDto>> ObtenerPorVarianteAsync(Guid idVariante);
    Task<SkuDto?> ObtenerPorIdAsync(Guid idSku);
    Task<SkuDto> CrearAsync(SkuCreateDto dto, string userEmail);
    Task<SkuDto?> ActualizarAsync(Guid idSku, SkuUpdateDto dto, string userEmail);
    Task<bool> EliminarAsync(Guid idSku);
    Task<IEnumerable<SkuCatalogoDto>> ObtenerTodosCatalogoAsync(Guid? idVariante, Guid? idProducto, bool? activo, bool? soloConStock);
}

public class SkuService(AppDbContext dbContext) : ISkuService
{
    public async Task<IEnumerable<SkuDto>> ObtenerTodosAsync()
    {
        var skus = await dbContext.Skus
            .AsNoTracking()
            .ToListAsync();

        return skus.Select(MapToDto);
    }

    public async Task<IEnumerable<SkuCatalogoDto>> ObtenerTodosCatalogoAsync(Guid? idVariante, Guid? idProducto, bool? activo, bool? soloConStock)
    {
        var query = dbContext.Skus
            .Include(s => s.Variante)
                .ThenInclude(v => v.Producto)
            .Include(s => s.Talla)
                .ThenInclude(t => t.ElementoPadre)
            .AsQueryable();

        if (idVariante.HasValue)
        {
            query = query.Where(s => s.IdVariante == idVariante.Value);
        }

        if (idProducto.HasValue)
        {
            query = query.Where(s => s.Variante!.IdProducto == idProducto.Value);
        }

        if (activo.HasValue)
        {
            var estatus = activo.Value ? "ACTIVO" : "INACTIVO";
            query = query.Where(s => s.ClEstatusSku == estatus);
        }

        if (soloConStock.HasValue && soloConStock.Value)
        {
            query = query.Where(s => (s.NoStockDisponible - s.NoStockReservado) > 0);
        }

        var results = await query.AsNoTracking().ToListAsync();

        return results.Select(s => new SkuCatalogoDto(
            s.IdSku,
            s.IdVariante,
            s.Variante!.IdProducto,
            s.Variante!.Producto!.NbProducto,
            s.IdElemTalla,
            s.Talla?.NbCatalogoElemento ?? string.Empty,
            s.Talla?.ElementoPadre?.NbCatalogoElemento ?? string.Empty,
            s.ClEstatusSku == "ACTIVO",
            s.NoStockDisponible,
            s.NoStockReservado,
            s.NoStockDisponible - s.NoStockReservado,
            s.FeCreacion,
            s.FeModificacion
        ));
    }

    public async Task<IEnumerable<SkuDto>> ObtenerPorVarianteAsync(Guid idVariante)
    {
        var skus = await dbContext.Skus
            .Where(s => s.IdVariante == idVariante)
            .AsNoTracking()
            .ToListAsync();

        return skus.Select(MapToDto);
    }

    public async Task<SkuDto?> ObtenerPorIdAsync(Guid idSku)
    {
        var sku = await dbContext.Skus
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.IdSku == idSku);

        return sku == null ? null : MapToDto(sku);
    }

    public async Task<SkuDto> CrearAsync(SkuCreateDto dto, string userEmail)
    {
        var sku = new etSku
        {
            IdSku = Guid.NewGuid(),
            IdVariante = dto.IdVariante,
            IdElemTalla = dto.IdElemTalla,
            ClItem = dto.ClItem,
            ClCodigoBarras = dto.ClCodigoBarras,
            ClEstatusSku = dto.ClEstatusSku,
            NoStockDisponible = dto.NoStockDisponible,
            NoStockReservado = dto.NoStockReservado,
            ClOperadorCrea = userEmail,
            NbArtefactoCrea = "SkuService.CrearAsync"
        };

        dbContext.Skus.Add(sku);
        await dbContext.SaveChangesAsync();

        return MapToDto(sku);
    }

    public async Task<SkuDto?> ActualizarAsync(Guid idSku, SkuUpdateDto dto, string userEmail)
    {
        var sku = await dbContext.Skus
            .FirstOrDefaultAsync(s => s.IdSku == idSku);

        if (sku == null) return null;

        sku.IdElemTalla = dto.IdElemTalla;
        sku.ClItem = dto.ClItem;
        sku.ClCodigoBarras = dto.ClCodigoBarras;
        sku.ClEstatusSku = dto.ClEstatusSku;
        sku.NoStockDisponible = dto.NoStockDisponible;
        sku.NoStockReservado = dto.NoStockReservado;

        sku.ClOperadorModifica = userEmail;
        sku.NbArtefactoModifica = "SkuService.ActualizarAsync";
        sku.FeModificacion = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync();
        return MapToDto(sku);
    }

    public async Task<bool> EliminarAsync(Guid idSku)
    {
        var sku = await dbContext.Skus
            .FirstOrDefaultAsync(s => s.IdSku == idSku);

        if (sku == null) return false;

        dbContext.Skus.Remove(sku);
        await dbContext.SaveChangesAsync();
        return true;
    }

    private static SkuDto MapToDto(etSku s)
    {
        return new SkuDto(
            s.IdSku,
            s.IdVariante,
            s.IdElemTalla,
            s.ClItem,
            s.ClCodigoBarras,
            s.ClEstatusSku,
            s.NoStockDisponible,
            s.NoStockReservado,
            s.FeCreacion,
            s.FeModificacion
        );
    }
}
