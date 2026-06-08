using GestionPedidos.Contracts.Productos;
using GestionPedidos.Contracts.Skus;
using GestionPedidos.Contracts.Variantes;
using GestionPedidos.Data;
using GestionPedidos.Models.Catalogo;
using GestionPedidos.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPedidos.Services;

public interface IProductoGuanteService
{
    Task<IEnumerable<ProductoGuanteDto>> ObtenerTodosAsync();
    Task<ProductoGuanteDto?> ObtenerPorIdAsync(Guid idProducto);
    Task<ProductoGuanteDto> CrearAsync(ProductoGuanteCreateDto dto, string userEmail);
    Task<ProductoGuanteDto?> ActualizarAsync(Guid idProducto, ProductoGuanteUpdateDto dto, string userEmail);
    Task<bool> EliminarAsync(Guid idProducto);
}

public class ProductoGuanteService(AppDbContext dbContext) : IProductoGuanteService
{
    public async Task<IEnumerable<ProductoGuanteDto>> ObtenerTodosAsync()
    {
        var guantes = await dbContext.ProductosGuante
            .Include(g => g.Producto)
                .ThenInclude(p => p.Variantes)
                    .ThenInclude(v => v.Skus)
            .AsNoTracking()
            .ToListAsync();

        return guantes.Select(MapToDto);
    }

    public async Task<ProductoGuanteDto?> ObtenerPorIdAsync(Guid idProducto)
    {
        var guante = await dbContext.ProductosGuante
            .Include(g => g.Producto)
                .ThenInclude(p => p.Variantes)
                    .ThenInclude(v => v.Skus)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.IdProducto == idProducto);

        return guante == null ? null : MapToDto(guante);
    }

    public async Task<ProductoGuanteDto> CrearAsync(ProductoGuanteCreateDto dto, string userEmail)
    {
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var producto = new etProducto
            {
                IdProducto = Guid.NewGuid(),
                ClProducto = dto.ClProducto,
                NbProducto = dto.NbProducto,
                IdElemDivision = dto.IdElemDivision,
                IdElemLineaColeccion = dto.IdElemLineaColeccion,
                IdElemGama = dto.IdElemGama,
                ClHsCode = dto.ClHsCode,
                ClEstatusProducto = dto.ClEstatusProducto,
                ClOperadorCrea = userEmail,
                NbArtefactoCrea = "ProductoGuanteService.CrearAsync"
            };

            dbContext.Productos.Add(producto);

            var guante = new etProductoGuante
            {
                IdProducto = producto.IdProducto,
                NbPalma = dto.NbPalma,
                DsComposicion = dto.DsComposicion,
                ClMsCode = dto.ClMsCode,
                ClIndicePalma = dto.ClIndicePalma,
                DsForro = dto.DsForro,
                DsCierre = dto.DsCierre,
                DsHomologacion = dto.DsHomologacion,
                Producto = producto
            };

            dbContext.ProductosGuante.Add(guante);

            if (dto.Variantes != null && dto.Variantes.Any())
            {
                foreach (var varDto in dto.Variantes)
                {
                    var variante = new etVariante
                    {
                        IdVariante = Guid.NewGuid(),
                        IdProducto = producto.IdProducto,
                        IdElemCombinacion = varDto.IdElemCombinacion,
                        UrlImagen = varDto.UrlImagen,
                        ClEstatusVariante = varDto.ClEstatusVariante,
                        ClOperadorCrea = userEmail,
                        NbArtefactoCrea = "ProductoGuanteService.CrearAsync"
                    };
                    dbContext.Variantes.Add(variante);

                    if (varDto.Skus != null && varDto.Skus.Any())
                    {
                        foreach (var skuDto in varDto.Skus)
                        {
                            var sku = new etSku
                            {
                                IdSku = Guid.NewGuid(),
                                IdVariante = variante.IdVariante,
                                IdElemTalla = skuDto.IdElemTalla,
                                ClItem = skuDto.ClItem,
                                ClCodigoBarras = skuDto.ClCodigoBarras,
                                ClEstatusSku = skuDto.ClEstatusSku,
                                NoStockDisponible = skuDto.NoStockDisponible,
                                NoStockReservado = skuDto.NoStockReservado,
                                ClOperadorCrea = userEmail,
                                NbArtefactoCrea = "ProductoGuanteService.CrearAsync"
                            };
                            dbContext.Skus.Add(sku);
                        }
                    }
                }
            }

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return await ObtenerPorIdAsync(producto.IdProducto) ?? MapToDto(guante);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ProductoGuanteDto?> ActualizarAsync(Guid idProducto, ProductoGuanteUpdateDto dto, string userEmail)
    {
        var guante = await dbContext.ProductosGuante
            .Include(g => g.Producto)
                .ThenInclude(p => p.Variantes)
                    .ThenInclude(v => v.Skus)
            .FirstOrDefaultAsync(g => g.IdProducto == idProducto);

        if (guante == null) return null;

        using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            // Actualizar datos base del Producto
            guante.Producto.ClProducto = dto.ClProducto;
            guante.Producto.NbProducto = dto.NbProducto;
            guante.Producto.IdElemDivision = dto.IdElemDivision;
            guante.Producto.IdElemLineaColeccion = dto.IdElemLineaColeccion;
            guante.Producto.IdElemGama = dto.IdElemGama;
            guante.Producto.ClHsCode = dto.ClHsCode;
            guante.Producto.ClEstatusProducto = dto.ClEstatusProducto;
            guante.Producto.ClOperadorModifica = userEmail;
            guante.Producto.NbArtefactoModifica = "ProductoGuanteService.ActualizarAsync";
            guante.Producto.FeModificacion = DateTimeOffset.UtcNow;

            // Actualizar propiedades del Guante
            guante.NbPalma = dto.NbPalma;
            guante.DsComposicion = dto.DsComposicion;
            guante.ClMsCode = dto.ClMsCode;
            guante.ClIndicePalma = dto.ClIndicePalma;
            guante.DsForro = dto.DsForro;
            guante.DsCierre = dto.DsCierre;
            guante.DsHomologacion = dto.DsHomologacion;

            // Procesamiento de Variantes
            if (dto.Variantes != null)
            {
                var inputVarianteIds = dto.Variantes
                    .Where(v => v.IdVariante.HasValue)
                    .Select(v => v.IdVariante!.Value)
                    .ToList();

                // 1. Eliminar Variantes omitidas en el JSON
                var variantesAEliminar = guante.Producto.Variantes
                    .Where(v => !inputVarianteIds.Contains(v.IdVariante))
                    .ToList();

                foreach (var varAEliminar in variantesAEliminar)
                {
                    // Debemos eliminar explícitamente sus SKUs hijos primero debido a Restrict
                    dbContext.Skus.RemoveRange(varAEliminar.Skus);
                    dbContext.Variantes.Remove(varAEliminar);
                }

                // 2. Procesar inserciones y actualizaciones
                foreach (var inputVar in dto.Variantes)
                {
                    if (inputVar.IdVariante.HasValue && inputVarianteIds.Contains(inputVar.IdVariante.Value))
                    {
                        // Actualizar variante existente
                        var existingVar = guante.Producto.Variantes.FirstOrDefault(v => v.IdVariante == inputVar.IdVariante.Value);
                        if (existingVar != null)
                        {
                            existingVar.IdElemCombinacion = inputVar.IdElemCombinacion;
                            existingVar.UrlImagen = inputVar.UrlImagen;
                            existingVar.ClEstatusVariante = inputVar.ClEstatusVariante;
                            existingVar.ClOperadorModifica = userEmail;
                            existingVar.FeModificacion = DateTimeOffset.UtcNow;
                            existingVar.NbArtefactoModifica = "ProductoGuanteService.ActualizarAsync";

                            // Procesar SKUs de la variante existente
                            if (inputVar.Skus != null)
                            {
                                var inputSkuIds = inputVar.Skus.Where(s => s.IdSku.HasValue).Select(s => s.IdSku!.Value).ToList();
                                
                                // Eliminar SKUs omitidos
                                var skusAEliminar = existingVar.Skus.Where(s => !inputSkuIds.Contains(s.IdSku)).ToList();
                                dbContext.Skus.RemoveRange(skusAEliminar);

                                foreach (var inputSku in inputVar.Skus)
                                {
                                    if (inputSku.IdSku.HasValue && inputSkuIds.Contains(inputSku.IdSku.Value))
                                    {
                                        var existingSku = existingVar.Skus.FirstOrDefault(s => s.IdSku == inputSku.IdSku.Value);
                                        if (existingSku != null)
                                        {
                                            existingSku.IdElemTalla = inputSku.IdElemTalla;
                                            existingSku.ClItem = inputSku.ClItem;
                                            existingSku.ClCodigoBarras = inputSku.ClCodigoBarras;
                                            existingSku.ClEstatusSku = inputSku.ClEstatusSku;
                                            existingSku.NoStockDisponible = inputSku.NoStockDisponible;
                                            existingSku.NoStockReservado = inputSku.NoStockReservado;
                                            existingSku.ClOperadorModifica = userEmail;
                                            existingSku.FeModificacion = DateTimeOffset.UtcNow;
                                            existingSku.NbArtefactoModifica = "ProductoGuanteService.ActualizarAsync";
                                        }
                                    }
                                    else
                                    {
                                        // Crear SKU nuevo para la variante existente
                                        var nuevoSku = new etSku
                                        {
                                            IdSku = Guid.NewGuid(),
                                            IdVariante = existingVar.IdVariante,
                                            IdElemTalla = inputSku.IdElemTalla,
                                            ClItem = inputSku.ClItem,
                                            ClCodigoBarras = inputSku.ClCodigoBarras,
                                            ClEstatusSku = inputSku.ClEstatusSku,
                                            NoStockDisponible = inputSku.NoStockDisponible,
                                            NoStockReservado = inputSku.NoStockReservado,
                                            ClOperadorCrea = userEmail,
                                            NbArtefactoCrea = "ProductoGuanteService.ActualizarAsync"
                                        };
                                        dbContext.Skus.Add(nuevoSku);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Inserción de variante totalmente nueva
                        var nuevaVariante = new etVariante
                        {
                            IdVariante = Guid.NewGuid(),
                            IdProducto = guante.IdProducto,
                            IdElemCombinacion = inputVar.IdElemCombinacion,
                            UrlImagen = inputVar.UrlImagen,
                            ClEstatusVariante = inputVar.ClEstatusVariante,
                            ClOperadorCrea = userEmail,
                            NbArtefactoCrea = "ProductoGuanteService.ActualizarAsync"
                        };
                        dbContext.Variantes.Add(nuevaVariante);

                        if (inputVar.Skus != null && inputVar.Skus.Any())
                        {
                            foreach (var skuDto in inputVar.Skus)
                            {
                                var nuevoSku = new etSku
                                {
                                    IdSku = Guid.NewGuid(),
                                    IdVariante = nuevaVariante.IdVariante,
                                    IdElemTalla = skuDto.IdElemTalla,
                                    ClItem = skuDto.ClItem,
                                    ClCodigoBarras = skuDto.ClCodigoBarras,
                                    ClEstatusSku = skuDto.ClEstatusSku,
                                    NoStockDisponible = skuDto.NoStockDisponible,
                                    NoStockReservado = skuDto.NoStockReservado,
                                    ClOperadorCrea = userEmail,
                                    NbArtefactoCrea = "ProductoGuanteService.ActualizarAsync"
                                };
                                dbContext.Skus.Add(nuevoSku);
                            }
                        }
                    }
                }
            }

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return await ObtenerPorIdAsync(idProducto); // Recargar
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> EliminarAsync(Guid idProducto)
    {
        // Se debe incluir el árbol completo para eliminar en cascada desde código 
        // debido a que tenemos configurado DeleteBehavior.Restrict en el DbContext
        var guante = await dbContext.ProductosGuante
            .Include(g => g.Producto)
                .ThenInclude(p => p.Variantes)
                    .ThenInclude(v => v.Skus)
            .FirstOrDefaultAsync(g => g.IdProducto == idProducto);

        if (guante == null) return false;

        using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            foreach (var variante in guante.Producto.Variantes)
            {
                if (variante.Skus.Any())
                {
                    dbContext.Skus.RemoveRange(variante.Skus);
                }
                dbContext.Variantes.Remove(variante);
            }

            dbContext.ProductosGuante.Remove(guante);
            dbContext.Productos.Remove(guante.Producto);
            
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static ProductoGuanteDto MapToDto(etProductoGuante g)
    {
        var variantesDto = g.Producto.Variantes?.Select(v => new VarianteDto(
            v.IdVariante,
            v.IdProducto,
            v.IdElemCombinacion,
            v.UrlImagen,
            v.ClEstatusVariante,
            v.FeCreacion,
            v.FeModificacion,
            v.Skus?.Select(s => new SkuDto(
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
            )).ToList()
        )).ToList() ?? new List<VarianteDto>();

        return new ProductoGuanteDto(
            g.IdProducto,
            g.Producto.ClProducto,
            g.Producto.NbProducto,
            g.Producto.IdElemDivision,
            g.Producto.IdElemLineaColeccion,
            g.Producto.IdElemGama,
            g.Producto.ClHsCode,
            g.Producto.ClEstatusProducto,
            g.NbPalma,
            g.DsComposicion,
            g.ClMsCode,
            g.ClIndicePalma,
            g.DsForro,
            g.DsCierre,
            g.DsHomologacion,
            g.Producto.FeCreacion,
            g.Producto.FeModificacion,
            variantesDto
        );
    }
}
