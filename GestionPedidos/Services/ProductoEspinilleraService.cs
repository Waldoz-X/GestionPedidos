using GestionPedidos.Contracts.Productos;
using GestionPedidos.Contracts.Skus;
using GestionPedidos.Contracts.Variantes;
using GestionPedidos.Data;
using GestionPedidos.Models.Catalogo;
using GestionPedidos.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPedidos.Services;

public interface IProductoEspinilleraService
{
    Task<IEnumerable<ProductoEspinilleraDto>> ObtenerTodosAsync();
    Task<ProductoEspinilleraDto?> ObtenerPorIdAsync(Guid idProducto);
    Task<ProductoEspinilleraDto> CrearAsync(ProductoEspinilleraCreateDto dto, string userEmail);
    Task<ProductoEspinilleraDto?> ActualizarAsync(Guid idProducto, ProductoEspinilleraUpdateDto dto, string userEmail);
    Task<bool> EliminarAsync(Guid idProducto);
    Task<int> CrearMasivoAsync(IEnumerable<ProductoEspinilleraBulkDto> dtos, string userEmail);
}

public class ProductoEspinilleraService(AppDbContext dbContext) : IProductoEspinilleraService
{
    public async Task<IEnumerable<ProductoEspinilleraDto>> ObtenerTodosAsync()
    {
        var espinilleras = await dbContext.ProductosEspinillera
            .Include(g => g.Producto)
                .ThenInclude(p => p.Variantes)
                    .ThenInclude(v => v.Skus)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();

        return espinilleras.Select(MapToDto);
    }

    public async Task<ProductoEspinilleraDto?> ObtenerPorIdAsync(Guid idProducto)
    {
        var espinillera = await dbContext.ProductosEspinillera
            .Include(g => g.Producto)
                .ThenInclude(p => p.Variantes)
                    .ThenInclude(v => v.Skus)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(g => g.IdProducto == idProducto);

        return espinillera == null ? null : MapToDto(espinillera);
    }

    public async Task<ProductoEspinilleraDto> CrearAsync(ProductoEspinilleraCreateDto dto, string userEmail)
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
                    NbArtefactoCrea = "ProductoEspinilleraService.CrearAsync"
                };

                dbContext.Productos.Add(producto);

                var espinillera = new etProductoEspinillera
                {
                    IdProducto = producto.IdProducto,
                    DsMaterial = dto.DsMaterial,
                    DsProteccion = dto.DsProteccion,
                    Producto = producto
                };

                dbContext.ProductosEspinillera.Add(espinillera);

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
                            NbArtefactoCrea = "ProductoEspinilleraService.CrearAsync"
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
                                    NbArtefactoCrea = "ProductoEspinilleraService.CrearAsync"
                                };
                                dbContext.Skus.Add(sku);
                            }
                        }
                    }
                }

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return await ObtenerPorIdAsync(producto.IdProducto) ?? MapToDto(espinillera);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public async Task<int> CrearMasivoAsync(IEnumerable<ProductoEspinilleraBulkDto> dtos, string userEmail)
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
        var espinilleras = new List<etProductoEspinillera>();
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
                NbArtefactoCrea = "ProductoEspinilleraService.CrearMasivoAsync"
            };
            productos.Add(producto);

            var espinillera = new etProductoEspinillera
            {
                IdProducto = producto.IdProducto,
                DsMaterial = dto.DsMaterial,
                DsProteccion = dto.DsProteccion,
                Producto = producto
            };
            espinilleras.Add(espinillera);

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
                        NbArtefactoCrea = "ProductoEspinilleraService.CrearMasivoAsync"
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
                                NbArtefactoCrea = "ProductoEspinilleraService.CrearMasivoAsync"
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
                dbContext.ProductosEspinillera.AddRange(espinilleras);
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

    public async Task<ProductoEspinilleraDto?> ActualizarAsync(Guid idProducto, ProductoEspinilleraUpdateDto dto, string userEmail)
    {
        var espinillera = await dbContext.ProductosEspinillera
            .Include(g => g.Producto)
                .ThenInclude(p => p.Variantes)
                    .ThenInclude(v => v.Skus)
            .FirstOrDefaultAsync(g => g.IdProducto == idProducto);

        if (espinillera == null) return null;

        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                espinillera.Producto.ClProducto = dto.ClProducto;
                espinillera.Producto.NbProducto = dto.NbProducto;
                espinillera.Producto.IdElemCategoria = dto.IdElemCategoria;
                espinillera.Producto.IdElemLineaColeccion = dto.IdElemLineaColeccion;
                espinillera.Producto.ClHsCode = dto.ClHsCode;
                espinillera.Producto.ClEstatusProducto = dto.ClEstatusProducto;
                espinillera.Producto.ClOperadorModifica = userEmail;
                espinillera.Producto.NbArtefactoModifica = "ProductoEspinilleraService.ActualizarAsync";
                espinillera.Producto.FeModificacion = DateTimeOffset.UtcNow;

                espinillera.DsMaterial = dto.DsMaterial;
                espinillera.DsProteccion = dto.DsProteccion;

                if (dto.Variantes != null)
                {
                    var inputVarianteIds = dto.Variantes
                        .Where(v => v.IdVariante.HasValue)
                        .Select(v => v.IdVariante!.Value)
                        .ToList();

                    var variantesAEliminar = espinillera.Producto.Variantes
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
                            var existingVar = espinillera.Producto.Variantes.FirstOrDefault(v => v.IdVariante == inputVar.IdVariante.Value);
                            if (existingVar != null)
                            {
                                existingVar.IdElemCombinacion = inputVar.IdElemCombinacion;
                                existingVar.UrlImagen = inputVar.UrlImagen;
                                existingVar.ClEstatusVariante = inputVar.ClEstatusVariante;
                                existingVar.ClOperadorModifica = userEmail;
                                existingVar.FeModificacion = DateTimeOffset.UtcNow;
                                existingVar.NbArtefactoModifica = "ProductoEspinilleraService.ActualizarAsync";

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
                                                existingSku.NbArtefactoModifica = "ProductoEspinilleraService.ActualizarAsync";
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
                                                NbArtefactoCrea = "ProductoEspinilleraService.ActualizarAsync"
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
                                IdProducto = espinillera.IdProducto,
                                IdElemCombinacion = inputVar.IdElemCombinacion,
                                UrlImagen = inputVar.UrlImagen,
                                ClEstatusVariante = inputVar.ClEstatusVariante,
                                ClOperadorCrea = userEmail,
                                NbArtefactoCrea = "ProductoEspinilleraService.ActualizarAsync"
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
                                        NbArtefactoCrea = "ProductoEspinilleraService.ActualizarAsync"
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
        var espinillera = await dbContext.ProductosEspinillera
            .Include(g => g.Producto)
                .ThenInclude(p => p.Variantes)
                    .ThenInclude(v => v.Skus)
            .FirstOrDefaultAsync(g => g.IdProducto == idProducto);

        if (espinillera == null) return false;

        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var variante in espinillera.Producto.Variantes)
                {
                    if (variante.Skus.Any())
                    {
                        dbContext.Skus.RemoveRange(variante.Skus);
                    }
                    dbContext.Variantes.Remove(variante);
                }

                dbContext.ProductosEspinillera.Remove(espinillera);
                dbContext.Productos.Remove(espinillera.Producto);

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

    private ProductoEspinilleraDto MapToDto(etProductoEspinillera g)
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

        return new ProductoEspinilleraDto(
            g.IdProducto,
            g.Producto.ClProducto,
            g.Producto.NbProducto,
            g.Producto.IdElemCategoria,
            g.Producto.IdElemLineaColeccion,
            g.Producto.ClHsCode,
            g.Producto.ClEstatusProducto,
            g.DsMaterial,
            g.DsProteccion,
            g.Producto.FeCreacion,
            g.Producto.FeModificacion,
            variantesDto
        );
    }
}
