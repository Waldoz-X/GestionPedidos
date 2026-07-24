using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GestionPedidos.Data;
using GestionPedidos.Models;

namespace GestionPedidos.Services.Workers;

public class LiberadorPedidosExpiradosWorker(
    IServiceProvider serviceProvider,
    ILogger<LiberadorPedidosExpiradosWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("LiberadorPedidosExpiradosWorker iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var strategy = dbContext.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);
                    try
                    {
                        // 1. Buscar pedidos en borrador expirados
                        var pedidosExpirados = await dbContext.Pedidos
                            .Include(p => p.Lineas)
                            .Where(p => p.ClEstatusPedido == "BORRADOR" 
                                     && p.FeExpiracion != null 
                                     && p.FeExpiracion < DateTimeOffset.UtcNow)
                            .ToListAsync(stoppingToken);

                        if (pedidosExpirados.Count > 0)
                        {
                            logger.LogInformation("Se encontraron {Count} pedidos expirados para liberar.", pedidosExpirados.Count);

                            var estatusExpirado = await dbContext.CCatalogoElementos
                                .FirstOrDefaultAsync(e => e.ClCatalogoElemento == "EXPIRADO" 
                                                       && e.Catalogo.ClCatalogo == "ESTATUS_PEDIDO", stoppingToken)
                                ?? throw new InvalidOperationException("El estatus 'EXPIRADO' no existe.");

                            var systemUser = await dbContext.Users
                                .FirstOrDefaultAsync(u => u.NormalizedEmail == "ADMIN@GP.LOCAL", stoppingToken);
                            var systemUserId = systemUser?.Id ?? Guid.Empty;

                            foreach (var pedido in pedidosExpirados)
                            {
                                logger.LogInformation("Liberando stock de pedido expirado Folio: {Folio}", pedido.ClFolio);

                                foreach (var linea in pedido.Lineas)
                                {
                                    // 2. Bloquear y recuperar el SKU
                                    var sku = await dbContext.Skus
                                        .FromSqlRaw("SELECT * FROM et_sku WITH (UPDLOCK, ROWLOCK) WHERE id_sku = {0}", linea.IdSku)
                                        .FirstOrDefaultAsync(stoppingToken);

                                    if (sku != null)
                                    {
                                        logger.LogInformation("  - Liberando {Qty} unidades del SKU {Item}", linea.NoCantidad, sku.ClItem);
                                        sku.NoStockReservado = Math.Max(0, sku.NoStockReservado - linea.NoCantidad);
                                        sku.FeModificacion = DateTimeOffset.UtcNow;
                                        sku.ClOperadorModifica = "LiberadorWorker";
                                        sku.NbArtefactoModifica = "LiberadorPedidosExpiradosWorker";
                                    }
                                }

                                var estatusAnterior = await dbContext.CCatalogoElementos
                                    .FirstOrDefaultAsync(e => e.ClCatalogoElemento == pedido.ClEstatusPedido 
                                                           && e.Catalogo.ClCatalogo == "ESTATUS_PEDIDO", stoppingToken);

                                pedido.ClEstatusPedido = "EXPIRADO";
                                pedido.FeExpiracion = null;
                                pedido.FeModificacion = DateTimeOffset.UtcNow;
                                pedido.ClOperadorModifica = "LiberadorWorker";
                                pedido.NbArtefactoModifica = "LiberadorPedidosExpiradosWorker";

                                // Registrar en el historial de pedidos
                                dbContext.HistorialPedidos.Add(new etHistorialPedido
                                {
                                    Id = Guid.NewGuid(),
                                    IdPedido = pedido.IdPedido,
                                    EstatusAnterior = estatusAnterior,
                                    IdElemEstatusAnterior = estatusAnterior?.IdCatalogoElemento,
                                    EstatusNuevo = estatusExpirado,
                                    IdElemEstatusNuevo = estatusExpirado.IdCatalogoElemento,
                                    IdUsuario = systemUserId, // Sistema/Admin
                                    Notas = "Liberado automáticamente por inactividad tras 20 minutos.",
                                    RegistradoEn = DateTimeOffset.UtcNow
                                });
                            }

                            await dbContext.SaveChangesAsync(stoppingToken);
                            await transaction.CommitAsync(stoppingToken);
                            logger.LogInformation("Stock de pedidos expirados liberado con éxito.");
                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            await transaction.RollbackAsync(stoppingToken);
                        }
                        catch (Exception rollbackEx)
                        {
                            logger.LogWarning(rollbackEx, "No se pudo revertir (Rollback) la transacción, probablemente ya esté cerrada.");
                        }
                        logger.LogError(ex, "Error procesando liberación de pedidos expirados en transacción.");
                    }
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en el ciclo del worker de liberación de pedidos.");
            }

            // Esperar 1 minuto antes del siguiente escaneo
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
