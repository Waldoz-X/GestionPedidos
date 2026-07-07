using GestionPedidos.Contracts.Productos;
using GestionPedidos.Contracts.Skus;
using GestionPedidos.Contracts.Variantes;
using GestionPedidos.Data;
using GestionPedidos.Models.Catalogo;
using GestionPedidos.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPedidos.Services;

public interface IProductoAccesorioService
{
    Task<IEnumerable<ProductoAccesorioDto>> ObtenerTodosAsync();
    Task<ProductoAccesorioDto?> ObtenerPorIdAsync(Guid idProducto);
    Task<ProductoAccesorioDto> CrearAsync(ProductoAccesorioCreateDto dto, string userEmail);
    Task<ProductoAccesorioDto?> ActualizarAsync(Guid idProducto, ProductoAccesorioUpdateDto dto, string userEmail);
    Task<bool> EliminarAsync(Guid idProducto);
    Task<int> CrearMasivoAsync(IEnumerable<ProductoAccesorioBulkDto> dtos, string userEmail);
}

public class ProductoAccesorioService(AppDbContext dbContext) : IProductoAccesorioService
{
    public async Task<IEnumerable<ProductoAccesorioDto>> ObtenerTodosAsync()
    {
        var accesorios = await dbContext.ProductosAccesorio
            .Include(g => g.Producto)
                .ThenInclude(p => p.Variantes)
                    .ThenInclude(v => v.Skus)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();

        return accesorios.Select(MapToDto);
    }

    public async Task<ProductoAccesorioDto?> ObtenerPorIdAsync(Guid idProducto)
    {
        var accesorio = await dbContext.ProductosAccesorio
            .Include(g => g.Producto)
                .ThenInclude(p => p.Variantes)
                    .ThenInclude(v => v.Skus)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(g => g.IdProducto == idProducto);

        return accesorio == null ? null : MapToDto(accesorio);
    }

    public async Task<ProductoAccesorioDto> CrearAsync(ProductoAccesorioCreateDto dto, string userEmail)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                var producto = new etProducto
                {
                    IdProducto = Guid.NewGuid(),
                    ClProducto = dto.ClProducto,
                    NbProducto = dto.NbProducto,
                    IdElemCategoria = dto.IdElemCategoria,
                    IdElemLineaColeccion = dto.IdElemLineaColeccion,
                    ClHsCode = dto.ClHsCode,
                    ClEstatusProducto = dto.ClEstatusProducto,
                    ClOperadorCrea = userEmail,
                    NbArtefactoCrea = "ProductoAccesorioService.CrearAsync"
                };

                dbContext.Productos.Add(producto);

                var accesorio = new etProductoAccesorio
                {
                    IdProducto = producto.IdProducto,
                    ClSubcategoria = dto.ClSubcategoria,
                    DsMaterialPrincipal = dto.DsMaterialPrincipal,
                    Producto = producto
                };

                dbContext.ProductosAccesorio.Add(accesorio);

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
                            NbArtefactoCrea = "ProductoAccesorioService.CrearAsync"
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
                                    NbArtefactoCrea = "ProductoAccesorioService.CrearAsync"
                                };
                                dbContext.Skus.Add(sku);
                            }
                        }
                    }
                }

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return await ObtenerPorIdAsync(producto.IdProducto) ?? MapToDto(accesorio);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public async Task<int> CrearMasivoAsync(IEnumerable<ProductoAccesorioBulkDto> dtos, string userEmail)
    {
        var todosElementos = await dbContext.CCatalogoElementos
            .Include(e => e.Catalogo)
            .AsNoTracking()
            .ToListAsync();

        var categorias = todosElementos
            .Where(e => e.Catalogo.ClCatalogo == "DIVISIONES")
            .ToDictionary(e => e.ClCatalogoElemento, e => e.IdCatalogoElemento, StringComparer.OrdinalIgnoreCase);

        var lineasColeccion = todosElementos
            .Where(e => e.Catalogo.ClCatalogo == "LINEAS_COLECCION")
            .ToDictionary(e => e.ClCatalogoElemento, e => e.IdCatalogoElemento, StringComparer.OrdinalIgnoreCase);

        var combinaciones = todosElementos
            .Where(e => e.Catalogo.ClCatalogo == "COMBINACIONES")
            .ToDictionary(e => e.ClCatalogoElemento, e => e.IdCatalogoElemento, StringComparer.OrdinalIgnoreCase);

        var tallas = todosElementos
            .Where(e => e.Catalogo.ClCatalogo == "TALLAS")
            .ToDictionary(e => e.ClCatalogoElemento, e => e.IdCatalogoElemento, StringComparer.OrdinalIgnoreCase);

        var errores = new List<string>();
        var productos = new List<etProducto>();
        var accesorios = new List<etProductoAccesorio>();
        var variantes = new List<etVariante>();
        var skus = new List<etSku>();
        int fila = 0;

        foreach (var dto in dtos)
        {
            fila++;

            if (!categorias.TryGetValue(dto.ClCategoria, out var idCategoria))
            {
                errores.Add($"Fila {fila} ({dto.ClProducto}): Categoría '{dto.ClCategoria}' no encontrada en catálogo DIVISIONES.");
                continue;
            }

            int? idLineaColeccion = null;
            if (!string.IsNullOrWhiteSpace(dto.ClLineaColeccion))
            {
                if (!lineasColeccion.TryGetValue(dto.ClLineaColeccion, out var idLinea))
                {
                    errores.Add($"Fila {fila} ({dto.ClProducto}): Línea/Colección '{dto.ClLineaColeccion}' no encontrada en catálogo LINEAS_COLECCION.");
                    continue;
                }
                idLineaColeccion = idLinea;
            }

            var producto = new etProducto
            {
                IdProducto = Guid.NewGuid(),
                ClProducto = dto.ClProducto,
                NbProducto = dto.NbProducto,
                IdElemCategoria = idCategoria,
                IdElemLineaColeccion = idLineaColeccion,
                ClHsCode = dto.ClHsCode,
                ClEstatusProducto = dto.ClEstatusProducto,
                ClOperadorCrea = userEmail,
                NbArtefactoCrea = "ProductoAccesorioService.CrearMasivoAsync"
            };
            productos.Add(producto);

            var accesorio = new etProductoAccesorio
            {
                IdProducto = producto.IdProducto,
                ClSubcategoria = dto.ClSubcategoria,
                DsMaterialPrincipal = dto.DsMaterialPrincipal,
                Producto = producto
            };
            accesorios.Add(accesorio);

            if (dto.Variantes != null && dto.Variantes.Any())
            {
                foreach (var varDto in dto.Variantes)
                {
                    int? idCombinacion = null;
                    if (!string.IsNullOrWhiteSpace(varDto.ClCombinacion))
                    {
                        if (!combinaciones.TryGetValue(varDto.ClCombinacion, out var idCombo))
                        {
                            errores.Add($"Fila {fila} ({dto.ClProducto}): Combinación '{varDto.ClCombinacion}' no encontrada en catálogo COMBINACIONES.");
                            continue;
                        }
                        idCombinacion = idCombo;
                    }

                    var variante = new etVariante
                    {
                        IdVariante = Guid.NewGuid(),
                        IdProducto = producto.IdProducto,
                        IdElemCombinacion = idCombinacion,
                        UrlImagen = varDto.UrlImagen,
                        ClEstatusVariante = varDto.ClEstatusVariante,
                        ClOperadorCrea = userEmail,
                        NbArtefactoCrea = "ProductoAccesorioService.CrearMasivoAsync"
                    };
                    variantes.Add(variante);

                    if (varDto.Skus != null && varDto.Skus.Any())
                    {
                        foreach (var skuDto in varDto.Skus)
                        {
                            int? idTalla = null;
                            if (!string.IsNullOrWhiteSpace(skuDto.ClTalla))
                            {
                                if (!tallas.TryGetValue(skuDto.ClTalla, out var idT))
                                {
                                    errores.Add($"Fila {fila} ({dto.ClProducto}): Talla '{skuDto.ClTalla}' no encontrada en catálogo TALLAS.");
                                    continue;
                                }
                                idTalla = idT;
                            }

                            var sku = new etSku
                            {
                                IdSku = Guid.NewGuid(),
                                IdVariante = variante.IdVariante,
                                IdElemTalla = idTalla,
                                ClItem = skuDto.ClItem,
                                ClCodigoBarras = skuDto.ClCodigoBarras,
                                ClEstatusSku = skuDto.ClEstatusSku,
                                NoStockDisponible = skuDto.NoStockDisponible,
                                NoStockReservado = skuDto.NoStockReservado,
                                ClOperadorCrea = userEmail,
                                NbArtefactoCrea = "ProductoAccesorioService.CrearMasivoAsync"
                            };
                            skus.Add(sku);
                        }
                    }
                }
            }
        }

        if (errores.Any())
        {
            throw new InvalidOperationException(
                $"Se encontraron {errores.Count} errores de mapeo en la carga masiva:\n" +
                string.Join("\n", errores));
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                dbContext.Productos.AddRange(productos);
                dbContext.ProductosAccesorio.AddRange(accesorios);
                dbContext.Variantes.AddRange(variantes);
                dbContext.Skus.AddRange(skus);

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return productos.Count;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public async Task<ProductoAccesorioDto?> ActualizarAsync(Guid idProducto, ProductoAccesorioUpdateDto dto, string userEmail)
    {
        var accesorio = await dbContext.ProductosAccesorio
            .Include(g => g.Producto)
                .ThenInclude(p => p.Variantes)
                    .ThenInclude(v => v.Skus)
            .FirstOrDefaultAsync(g => g.IdProducto == idProducto);

        if (accesorio == null) return null;

        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                accesorio.Producto.ClProducto = dto.ClProducto;
                accesorio.Producto.NbProducto = dto.NbProducto;
                accesorio.Producto.IdElemCategoria = dto.IdElemCategoria;
                accesorio.Producto.IdElemLineaColeccion = dto.IdElemLineaColeccion;
                accesorio.Producto.ClHsCode = dto.ClHsCode;
                accesorio.Producto.ClEstatusProducto = dto.ClEstatusProducto;
                accesorio.Producto.ClOperadorModifica = userEmail;
                accesorio.Producto.NbArtefactoModifica = "ProductoAccesorioService.ActualizarAsync";
                accesorio.Producto.FeModificacion = DateTimeOffset.UtcNow;

                accesorio.ClSubcategoria = dto.ClSubcategoria;
                accesorio.DsMaterialPrincipal = dto.DsMaterialPrincipal;

                if (dto.Variantes != null)
                {
                    var inputVarianteIds = dto.Variantes
                        .Where(v => v.IdVariante.HasValue)
                        .Select(v => v.IdVariante!.Value)
                        .ToList();

                    var variantesAEliminar = accesorio.Producto.Variantes
                        .Where(v => !inputVarianteIds.Contains(v.IdVariante))
                        .ToList();

                    foreach (var varAEliminar in variantesAEliminar)
                    {
                        dbContext.Skus.RemoveRange(varAEliminar.Skus);
                        dbContext.Variantes.Remove(varAEliminar);
                    }

                    foreach (var inputVar in dto.Variantes)
                    {
                        if (inputVar.IdVariante.HasValue && inputVarianteIds.Contains(inputVar.IdVariante.Value))
                        {
                            var existingVar = accesorio.Producto.Variantes.FirstOrDefault(v => v.IdVariante == inputVar.IdVariante.Value);
                            if (existingVar != null)
                            {
                                existingVar.IdElemCombinacion = inputVar.IdElemCombinacion;
                                existingVar.UrlImagen = inputVar.UrlImagen;
                                existingVar.ClEstatusVariante = inputVar.ClEstatusVariante;
                                existingVar.ClOperadorModifica = userEmail;
                                existingVar.FeModificacion = DateTimeOffset.UtcNow;
                                existingVar.NbArtefactoModifica = "ProductoAccesorioService.ActualizarAsync";

                                if (inputVar.Skus != null)
                                {
                                    var inputSkuIds = inputVar.Skus.Where(s => s.IdSku.HasValue).Select(s => s.IdSku!.Value).ToList();
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
                                                existingSku.NbArtefactoModifica = "ProductoAccesorioService.ActualizarAsync";
                                            }
                                        }
                                        else
                                        {
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
                                                NbArtefactoCrea = "ProductoAccesorioService.ActualizarAsync"
                                            };
                                            dbContext.Skus.Add(nuevoSku);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            var nuevaVariante = new etVariante
                            {
                                IdVariante = Guid.NewGuid(),
                                IdProducto = accesorio.IdProducto,
                                IdElemCombinacion = inputVar.IdElemCombinacion,
                                UrlImagen = inputVar.UrlImagen,
                                ClEstatusVariante = inputVar.ClEstatusVariante,
                                ClOperadorCrea = userEmail,
                                NbArtefactoCrea = "ProductoAccesorioService.ActualizarAsync"
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
                                        NbArtefactoCrea = "ProductoAccesorioService.ActualizarAsync"
                                    };
                                    dbContext.Skus.Add(nuevoSku);
                                }
                            }
                        }
                    }
                }

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return await ObtenerPorIdAsync(idProducto);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public async Task<bool> EliminarAsync(Guid idProducto)
    {
        var accesorio = await dbContext.ProductosAccesorio
            .Include(g => g.Producto)
                .ThenInclude(p => p.Variantes)
                    .ThenInclude(v => v.Skus)
            .FirstOrDefaultAsync(g => g.IdProducto == idProducto);

        if (accesorio == null) return false;

        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var variante in accesorio.Producto.Variantes)
                {
                    if (variante.Skus.Any())
                    {
                        dbContext.Skus.RemoveRange(variante.Skus);
                    }
                    dbContext.Variantes.Remove(variante);
                }

                dbContext.ProductosAccesorio.Remove(accesorio);
                dbContext.Productos.Remove(accesorio.Producto);

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    private ProductoAccesorioDto MapToDto(etProductoAccesorio g)
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

        return new ProductoAccesorioDto(
            g.IdProducto,
            g.Producto.ClProducto,
            g.Producto.NbProducto,
            g.Producto.IdElemCategoria,
            g.Producto.IdElemLineaColeccion,
            g.Producto.ClHsCode,
            g.Producto.ClEstatusProducto,
            g.ClSubcategoria,
            g.DsMaterialPrincipal,
            g.Producto.FeCreacion,
            g.Producto.FeModificacion,
            variantesDto
        );
    }
}
