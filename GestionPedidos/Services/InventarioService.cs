using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GestionPedidos.Contracts.Inventario;
using GestionPedidos.Data;
using GestionPedidos.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPedidos.Services;

public class InventarioService : IInventarioService
{
    private readonly AppDbContext dbContext;

    public InventarioService(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    // ────────────────────────────────────────────────────────────────────────
    //  REGISTRAR ENTRADA (Carga de stock)
    // ────────────────────────────────────────────────────────────────────────
    public async Task<MovimientoInventarioDto> RegistrarEntradaAsync(RegistrarMovimientoRequest request, string userEmail)
    {
        if (request.NoCantidad <= 0)
            throw new ArgumentException("La cantidad a ingresar debe ser mayor a cero.", nameof(request));

        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                // Bloqueo pesimista sobre la fila del SKU
                var sku = await dbContext.Skus
                    .FromSqlRaw("SELECT * FROM et_sku WITH (UPDLOCK, ROWLOCK) WHERE id_sku = {0}", request.IdSku)
                    .FirstOrDefaultAsync()
                    ?? throw new InvalidOperationException("El SKU especificado no existe.");

                sku.NoStockDisponible += request.NoCantidad;
                sku.ClOperadorModifica = userEmail;
                sku.NbArtefactoModifica = "InventarioService.RegistrarEntradaAsync";
                sku.FeModificacion = DateTimeOffset.UtcNow;

                var movimiento = new etMovimientoInventario
                {
                    IdMovimiento = Guid.NewGuid(),
                    IdSku = sku.IdSku,
                    NoCantidad = request.NoCantidad,
                    ClTipoMovimiento = "ENTRADA",
                    DsMotivo = request.DsMotivo ?? "Entrada manual de inventario",
                    ClOperadorCrea = userEmail,
                    NbArtefactoCrea = "InventarioService.RegistrarEntradaAsync",
                    FeCreacion = DateTimeOffset.UtcNow
                };

                dbContext.MovimientosInventario.Add(movimiento);

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return MapToDto(movimiento);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    // ────────────────────────────────────────────────────────────────────────
    //  REGISTRAR BAJA (Merma / Salida física)
    // ────────────────────────────────────────────────────────────────────────
    public async Task<MovimientoInventarioDto> RegistrarBajaAsync(RegistrarMovimientoRequest request, string userEmail)
    {
        if (request.NoCantidad <= 0)
            throw new ArgumentException("La cantidad a dar de baja debe ser mayor a cero.", nameof(request));

        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                // Bloqueo pesimista sobre la fila del SKU
                var sku = await dbContext.Skus
                    .FromSqlRaw("SELECT * FROM et_sku WITH (UPDLOCK, ROWLOCK) WHERE id_sku = {0}", request.IdSku)
                    .FirstOrDefaultAsync()
                    ?? throw new InvalidOperationException("El SKU especificado no existe.");

                if (sku.NoStockDisponible < request.NoCantidad)
                    throw new InvalidOperationException(
                        $"Stock insuficiente para realizar la baja. Stock disponible actual: {sku.NoStockDisponible}, solicitado: {request.NoCantidad}.");

                sku.NoStockDisponible -= request.NoCantidad;
                sku.ClOperadorModifica = userEmail;
                sku.NbArtefactoModifica = "InventarioService.RegistrarBajaAsync";
                sku.FeModificacion = DateTimeOffset.UtcNow;

                var movimiento = new etMovimientoInventario
                {
                    IdMovimiento = Guid.NewGuid(),
                    IdSku = sku.IdSku,
                    NoCantidad = -request.NoCantidad, // Representa salida en el Kardex
                    ClTipoMovimiento = "BAJA",
                    DsMotivo = request.DsMotivo ?? "Baja/Merma manual de inventario",
                    ClOperadorCrea = userEmail,
                    NbArtefactoCrea = "InventarioService.RegistrarBajaAsync",
                    FeCreacion = DateTimeOffset.UtcNow
                };

                dbContext.MovimientosInventario.Add(movimiento);

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return MapToDto(movimiento);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    // ────────────────────────────────────────────────────────────────────────
    //  REGISTRAR AJUSTE (Conteo físico)
    // ────────────────────────────────────────────────────────────────────────
    public async Task<MovimientoInventarioDto> RegistrarAjusteAsync(RegistrarAjusteRequest request, string userEmail)
    {
        if (request.NoStockFisicoReal < 0)
            throw new ArgumentException("El stock físico real no puede ser negativo.", nameof(request));

        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                // Bloqueo pesimista sobre la fila del SKU
                var sku = await dbContext.Skus
                    .FromSqlRaw("SELECT * FROM et_sku WITH (UPDLOCK, ROWLOCK) WHERE id_sku = {0}", request.IdSku)
                    .FirstOrDefaultAsync()
                    ?? throw new InvalidOperationException("El SKU especificado no existe.");

                int delta = request.NoStockFisicoReal - sku.NoStockDisponible;

                sku.NoStockDisponible = request.NoStockFisicoReal;
                sku.ClOperadorModifica = userEmail;
                sku.NbArtefactoModifica = "InventarioService.RegistrarAjusteAsync";
                sku.FeModificacion = DateTimeOffset.UtcNow;

                var movimiento = new etMovimientoInventario
                {
                    IdMovimiento = Guid.NewGuid(),
                    IdSku = sku.IdSku,
                    NoCantidad = delta, // El diferencial (puede ser positivo o negativo)
                    ClTipoMovimiento = "AJUSTE",
                    DsMotivo = request.DsMotivo ?? $"Ajuste físico de inventario (anterior: {sku.NoStockDisponible - delta}, nuevo: {request.NoStockFisicoReal})",
                    ClOperadorCrea = userEmail,
                    NbArtefactoCrea = "InventarioService.RegistrarAjusteAsync",
                    FeCreacion = DateTimeOffset.UtcNow
                };

                dbContext.MovimientosInventario.Add(movimiento);

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return MapToDto(movimiento);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    // ────────────────────────────────────────────────────────────────────────
    //  CONSULTAR HISTORIAL (Kardex)
    // ────────────────────────────────────────────────────────────────────────
    public async Task<IEnumerable<MovimientoInventarioDto>> ObtenerKardexSkuAsync(Guid idSku)
    {
        var movimientos = await dbContext.MovimientosInventario
            .AsNoTracking()
            .Where(m => m.IdSku == idSku)
            .OrderByDescending(m => m.FeCreacion)
            .ToListAsync();

        return movimientos.Select(MapToDto);
    }

    // ────────────────────────────────────────────────────────────────────────
    //  LIBRO DE AUDITORÍA (Kardex Global)
    // ────────────────────────────────────────────────────────────────────────
    public async Task<IEnumerable<LibroAuditoriaDto>> ObtenerLibroAuditoriaAsync(string? clTipoMovimiento, DateTimeOffset? feInicio, DateTimeOffset? feFin)
    {
        var query = dbContext.MovimientosInventario
            .Include(m => m.Sku)
                .ThenInclude(s => s.Variante)
                    .ThenInclude(v => v.Producto)
            .Include(m => m.Sku)
                .ThenInclude(s => s.Variante)
                    .ThenInclude(v => v.Combinacion)
            .Include(m => m.Sku)
                .ThenInclude(s => s.Talla)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(clTipoMovimiento))
        {
            var cleanType = clTipoMovimiento.ToUpperInvariant();
            query = query.Where(m => m.ClTipoMovimiento == cleanType);
        }

        if (feInicio.HasValue)
        {
            query = query.Where(m => m.FeCreacion >= feInicio.Value);
        }

        if (feFin.HasValue)
        {
            query = query.Where(m => m.FeCreacion <= feFin.Value);
        }

        var list = await query
            .OrderByDescending(m => m.FeCreacion)
            .ToListAsync();

        return list.Select(m => new LibroAuditoriaDto(
            m.IdMovimiento,
            m.FeCreacion,
            m.ClTipoMovimiento,
            m.IdSku,
            m.Sku?.ClItem,
            m.Sku?.Variante?.Producto?.NbProducto,
            m.Sku?.Variante?.Combinacion?.NbCatalogoElemento,
            m.Sku?.Talla?.NbCatalogoElemento,
            m.NoCantidad,
            m.ClOperadorCrea,
            m.DsMotivo
        ));
    }

    // ────────────────────────────────────────────────────────────────────────
    //  STOCK REAL (Estado Actual de Inventarios con Semáforo)
    // ────────────────────────────────────────────────────────────────────────
    public async Task<IEnumerable<StockRealDto>> ObtenerStockRealAsync()
    {
        var skus = await dbContext.Skus
            .Include(s => s.Variante)
                .ThenInclude(v => v.Producto)
            .Include(s => s.Variante)
                .ThenInclude(v => v.Combinacion)
            .Include(s => s.Talla)
            .Include(s => s.Precios)
            .AsNoTracking()
            .ToListAsync();

        return skus.Select(s =>
        {
            var stockNeto = s.NoStockDisponible - s.NoStockReservado;

            // Obtener el precio general de lista
            var precioBase = s.Precios
                .Where(p => p.IdCliente == null && p.IdPolitica == null && p.ClEstatusPrecio == "ACTIVO")
                .OrderByDescending(p => p.FeVigenteDesde)
                .FirstOrDefault()?.MnPrecioNeto ?? 0m;

            // Semáforo de stock
            string semaforo = "STOCK_OK";
            if (s.NoStockDisponible <= 0)
            {
                semaforo = "SIN_STOCK";
            }
            else if (s.NoStockDisponible <= s.NoStockMinimo)
            {
                semaforo = "STOCK_BAJO";
            }

            return new StockRealDto(
                s.IdSku,
                s.ClItem,
                s.Variante?.Producto?.NbProducto,
                s.Variante?.Combinacion?.NbCatalogoElemento,
                s.Talla?.NbCatalogoElemento,
                s.NoStockDisponible,
                s.NoStockReservado,
                stockNeto,
                s.NoStockMinimo,
                precioBase,
                semaforo
            );
        });
    }

    // ────────────────────────────────────────────────────────────────────────
    //  REPORTE DE ROTACIÓN (KPI de Ventas)
    // ────────────────────────────────────────────────────────────────────────
    public async Task<IEnumerable<RotacionSkuDto>> ObtenerRotacionInventarioAsync(DateTimeOffset feInicio, DateTimeOffset feFin)
    {
        var ventas = await dbContext.MovimientosInventario
            .Include(m => m.Sku)
                .ThenInclude(s => s.Variante)
                    .ThenInclude(v => v.Producto)
            .Include(m => m.Sku)
                .ThenInclude(s => s.Variante)
                    .ThenInclude(v => v.Combinacion)
            .Include(m => m.Sku)
                .ThenInclude(s => s.Talla)
            .AsNoTracking()
            .Where(m => m.ClTipoMovimiento == "VENTA" && m.FeCreacion >= feInicio && m.FeCreacion <= feFin)
            .ToListAsync();

        var grouped = ventas
            .GroupBy(m => m.IdSku)
            .Select(g =>
            {
                var first = g.First();
                // Sumamos los absolutos de las ventas
                var totalVendidas = g.Sum(m => Math.Abs(m.NoCantidad));

                return new RotacionSkuDto(
                    g.Key,
                    first.Sku?.ClItem,
                    first.Sku?.Variante?.Producto?.NbProducto,
                    first.Sku?.Variante?.Combinacion?.NbCatalogoElemento,
                    first.Sku?.Talla?.NbCatalogoElemento,
                    totalVendidas
                );
            })
            .OrderByDescending(r => r.NoCantidadVendida)
            .ToList();

        return grouped;
    }


    private static MovimientoInventarioDto MapToDto(etMovimientoInventario m)
    {
        return new MovimientoInventarioDto(
            m.IdMovimiento,
            m.IdSku,
            m.NoCantidad,
            m.ClTipoMovimiento,
            m.DsMotivo,
            m.ClOperadorCrea,
            m.FeCreacion
        );
    }
}
