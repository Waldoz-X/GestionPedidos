using GestionPedidos.Contracts.Pedidos;
using GestionPedidos.Data;
using GestionPedidos.Models;
using GestionPedidos.Models.Catalogo;
using Microsoft.EntityFrameworkCore;

namespace GestionPedidos.Services;

// ════════════════════════════════════════════════════════════════════════════
//  INTERFAZ
// ════════════════════════════════════════════════════════════════════════════
public interface IPedidoService
{
    // ── Cliente ──
    Task<PedidoDetalleDto> CrearBorradorAsync(CrearPedidoRequest request, Guid idCliente, Guid idUsuario, string userEmail);
    Task<LineaPedidoDto> AgregarLineaAsync(Guid idPedido, AgregarLineaRequest request, Guid idCliente, string userEmail);
    Task<LineaPedidoDto> ActualizarLineaAsync(Guid idPedido, Guid idLinea, ActualizarLineaRequest request, Guid idCliente, string userEmail);
    Task<bool> EliminarLineaAsync(Guid idPedido, Guid idLinea, Guid idCliente);
    Task<PedidoDetalleDto> ConfirmarPedidoAsync(Guid idPedido, Guid idCliente, Guid idUsuario, string userEmail);
    Task<bool> CancelarBorradorAsync(Guid idPedido, Guid idCliente);

    // ── Admin / Empleado asignado ──
    Task<PedidoDetalleDto> CambiarEstatusAsync(Guid idPedido, CambiarEstatusRequest request, Guid idUsuario, string userEmail);
    Task<bool> EmpleadoTieneAccesoAlPedido(Guid idUsuario, Guid idPedido);

    // ── Consultas ──
    Task<IEnumerable<PedidoResumenDto>> ObtenerPedidosClienteAsync(Guid idCliente);
    Task<PedidoDetalleDto?> ObtenerPedidoDetalleAsync(Guid idPedido);
    Task<IEnumerable<PedidoResumenDto>> ObtenerTodosPedidosAsync(string? filtroEstatus, Guid? filtroCliente, Guid? filtroEmpleado, Guid idUsuarioSolicitante, bool esAdmin);
    Task<DashboardResumenDto> ObtenerDashboardResumenAsync();
    Task ForzarExpiracionAsync(Guid idPedido);
}

// ════════════════════════════════════════════════════════════════════════════
//  IMPLEMENTACIÓN
// ════════════════════════════════════════════════════════════════════════════
public class PedidoService(AppDbContext dbContext, IPrecioService precioService, IVisibilidadService visibilidadService) : IPedidoService
{
    private static readonly Dictionary<string, string[]> TransicionesValidas = new()
    {
        ["BORRADOR"]   = ["CONFIRMADO", "CANCELADO", "EXPIRADO"],
        ["CONFIRMADO"] = ["FACTURADO", "CANCELADO"],
        ["FACTURADO"]  = ["ENVIADO"],
        ["ENVIADO"]    = [],
        ["CANCELADO"]  = [],
        ["EXPIRADO"]   = []
    };

    // ────────────────────────────────────────────────────────────────────────
    //  CREAR BORRADOR
    // ────────────────────────────────────────────────────────────────────────
    public async Task<PedidoDetalleDto> CrearBorradorAsync(
        CrearPedidoRequest request, Guid idCliente, Guid idUsuario, string userEmail)
    {
        // Validar dirección de envío pertenece al cliente (si se proporcionó)
        if (request.IdDireccionEnvio.HasValue)
        {
            var direccionValida = await dbContext.DireccionesCliente
                .AnyAsync(d => d.IdDireccion == request.IdDireccionEnvio.Value
                            && d.IdCliente == idCliente
                            && d.ClEstatusDireccion == "ACTIVO");
            if (!direccionValida)
                throw new InvalidOperationException("La dirección de envío no es válida para este cliente.");
        }

        // Resolver moneda del cliente
        var cliente = await dbContext.Clientes
            .Include(c => c.ClMoneda)
            .FirstOrDefaultAsync(c => c.IdCliente == idCliente)
            ?? throw new InvalidOperationException("Cliente no encontrado.");

        // Resolver política principal del cliente (si tiene)
        var politicaPrincipal = await dbContext.ClientesPoliticas
            .Include(cp => cp.Politica)
            .Where(cp => cp.IdCliente == idCliente && cp.EsPrincipal)
            .Select(cp => cp.Politica)
            .FirstOrDefaultAsync();

        // Generar folio thread-safe
        var folio = await GenerarFolioAsync();

        var pedido = new etPedido
        {
            IdPedido = Guid.NewGuid(),
            IdCliente = idCliente,
            IdUsuarioCaptura = idUsuario,
            IdDireccionEnvio = request.IdDireccionEnvio,
            IdPolitica = politicaPrincipal?.IdPolitica,
            ClFolio = folio,
            ClEstatusPedido = "BORRADOR",
            ClMoneda = cliente.ClMoneda,
            MnSubtotal = 0,
            MnDescuentoComercial = 0,
            MnDescuentoAdmin = 0,
            MnTotal = 0,
            FePedido = DateTimeOffset.UtcNow,
            FeExpiracion = DateTimeOffset.UtcNow.AddMinutes(20),
            ClOperadorCrea = userEmail,
            NbArtefactoCrea = "PedidoService.CrearBorradorAsync"
        };

        dbContext.Pedidos.Add(pedido);

        var estatusNuevoCatalogo = await ResolverEstatusCatalogoAsync("BORRADOR")
            ?? throw new InvalidOperationException("El estatus 'BORRADOR' no existe en la base de datos.");

        // Registrar historial: transición null → BORRADOR
        dbContext.HistorialPedidos.Add(new etHistorialPedido
        {
            Id = Guid.NewGuid(),
            IdPedido = pedido.IdPedido,
            EstatusAnterior = null,
            EstatusNuevo = estatusNuevoCatalogo,
            IdElemEstatusNuevo = estatusNuevoCatalogo.IdCatalogoElemento,
            IdUsuario = idUsuario,
            Notas = "Pedido creado como borrador",
            RegistradoEn = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync();

        return await ObtenerPedidoDetalleAsync(pedido.IdPedido)
            ?? throw new InvalidOperationException("Error al crear el pedido.");
    }

    // ────────────────────────────────────────────────────────────────────────
    //  AGREGAR LÍNEA
    // ────────────────────────────────────────────────────────────────────────
    public async Task<LineaPedidoDto> AgregarLineaAsync(
        Guid idPedido, AgregarLineaRequest request, Guid idCliente, string userEmail)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                var pedido = await ObtenerPedidoConValidacion(idPedido, idCliente, "BORRADOR");

                if (request.NoCantidad <= 0)
                    throw new InvalidOperationException("La cantidad debe ser mayor a 0.");

                // Cargar SKU con bloqueo pesimista
                var sku = await dbContext.Skus
                    .FromSqlRaw("SELECT * FROM et_sku WITH (UPDLOCK, ROWLOCK) WHERE id_sku = {0}", request.IdSku)
                    .FirstOrDefaultAsync()
                    ?? throw new InvalidOperationException($"El SKU '{request.IdSku}' no existe.");

                if (sku.ClEstatusSku != "ACTIVO")
                    throw new InvalidOperationException($"El SKU '{request.IdSku}' está inactivo.");

                // Cargar relaciones necesarias para el mapeo
                await dbContext.Entry(sku).Reference(s => s.Variante).LoadAsync();
                if (sku.Variante != null)
                {
                    await dbContext.Entry(sku.Variante).Reference(v => v.Producto).LoadAsync();
                    await dbContext.Entry(sku.Variante).Reference(v => v.Combinacion).LoadAsync();
                }
                await dbContext.Entry(sku).Reference(s => s.Talla).LoadAsync();

                if (sku.Variante == null)
                    throw new InvalidOperationException("El SKU no tiene una variante asociada.");

                // Validar visibilidad
                var productoVisible = await visibilidadService.TieneAccesoASkuAsync(
                    idCliente, 
                    sku.Variante.IdProducto, 
                    sku.IdVariante, 
                    sku.IdSku);
                if (!productoVisible)
                    throw new UnauthorizedAccessException("No tienes acceso a este producto (o variante/talla específica).");

                // Validar stock disponible neto
                var stockNeto = sku.NoStockDisponible - sku.NoStockReservado;
                if (request.NoCantidad > stockNeto)
                    throw new InvalidOperationException(
                        $"Stock insuficiente. Disponible: {stockNeto}, solicitado: {request.NoCantidad}.");

                // Incrementar reservado
                sku.NoStockReservado += request.NoCantidad;
                sku.ClOperadorModifica = userEmail;
                sku.NbArtefactoModifica = "PedidoService.AgregarLineaAsync.Reservar";
                sku.FeModificacion = DateTimeOffset.UtcNow;

                // Extender expiración
                pedido.FeExpiracion = DateTimeOffset.UtcNow.AddMinutes(20);
                pedido.ClOperadorModifica = userEmail;
                pedido.NbArtefactoModifica = "PedidoService.AgregarLineaAsync";
                pedido.FeModificacion = DateTimeOffset.UtcNow;

                var lineaExistente = await dbContext.LineasPedido
                    .FirstOrDefaultAsync(l => l.IdPedido == idPedido && l.IdSku == request.IdSku);

                LineaPedidoDto resultDto;
                if (lineaExistente != null)
                {
                    lineaExistente.NoCantidad += request.NoCantidad;
                    lineaExistente.MnSubtotal = lineaExistente.NoCantidad * lineaExistente.MnPrecioUnitario - lineaExistente.MnDescuentoLinea;
                    lineaExistente.ClOperadorModifica = userEmail;
                    lineaExistente.NbArtefactoModifica = "PedidoService.AgregarLineaAsync";
                    lineaExistente.FeModificacion = DateTimeOffset.UtcNow;

                    await RecalcularTotalesPedido(pedido);
                    await dbContext.SaveChangesAsync();
                    resultDto = MapToLineaDto(lineaExistente, sku);
                }
                else
                {
                    var precioResuelto = await precioService.ResolverPrecioAsync(request.IdSku, idCliente);
                    if (!precioResuelto.PrecioEncontrado || precioResuelto.PrecioFinal == null)
                        throw new InvalidOperationException(
                            $"No hay precio configurado para el SKU '{sku.ClItem ?? request.IdSku.ToString()}'. Contacte al administrador.");

                    var subtotalLinea = request.NoCantidad * precioResuelto.PrecioFinal.Value;

                    var linea = new etLineaPedido
                    {
                        IdLineaPedido = Guid.NewGuid(),
                        IdPedido = idPedido,
                        IdSku = request.IdSku,
                        NoCantidad = request.NoCantidad,
                        MnPrecioUnitario = precioResuelto.PrecioFinal.Value,
                        MnDescuentoLinea = 0,
                        MnSubtotal = subtotalLinea,
                        ClOperadorCrea = userEmail,
                        NbArtefactoCrea = "PedidoService.AgregarLineaAsync"
                    };

                    dbContext.LineasPedido.Add(linea);
                    await RecalcularTotalesPedido(pedido);
                    await dbContext.SaveChangesAsync();
                    resultDto = MapToLineaDto(linea, sku);
                }

                await transaction.CommitAsync();
                return resultDto;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    // ────────────────────────────────────────────────────────────────────────
    //  ACTUALIZAR LÍNEA
    // ────────────────────────────────────────────────────────────────────────
    public async Task<LineaPedidoDto> ActualizarLineaAsync(
        Guid idPedido, Guid idLinea, ActualizarLineaRequest request, Guid idCliente, string userEmail)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                var pedido = await ObtenerPedidoConValidacion(idPedido, idCliente, "BORRADOR");

                if (request.NoCantidad <= 0)
                    throw new InvalidOperationException("La cantidad debe ser mayor a 0.");

                var linea = await dbContext.LineasPedido
                    .FirstOrDefaultAsync(l => l.IdLineaPedido == idLinea && l.IdPedido == idPedido)
                    ?? throw new InvalidOperationException("Línea de pedido no encontrada.");

                // Bloqueo de SKU
                var sku = await dbContext.Skus
                    .FromSqlRaw("SELECT * FROM et_sku WITH (UPDLOCK, ROWLOCK) WHERE id_sku = {0}", linea.IdSku)
                    .FirstOrDefaultAsync()
                    ?? throw new InvalidOperationException($"El SKU '{linea.IdSku}' no existe.");

                var delta = request.NoCantidad - linea.NoCantidad;
                if (delta > 0)
                {
                    var stockNeto = sku.NoStockDisponible - sku.NoStockReservado;
                    if (delta > stockNeto)
                        throw new InvalidOperationException(
                            $"Stock insuficiente. Disponible: {stockNeto}, solicitado adicional: {delta}.");
                }

                // Ajustar reserva de stock
                sku.NoStockReservado += delta;
                sku.ClOperadorModifica = userEmail;
                sku.NbArtefactoModifica = "PedidoService.ActualizarLineaAsync";
                sku.FeModificacion = DateTimeOffset.UtcNow;

                // Extender expiración
                pedido.FeExpiracion = DateTimeOffset.UtcNow.AddMinutes(20);
                pedido.ClOperadorModifica = userEmail;
                pedido.NbArtefactoModifica = "PedidoService.ActualizarLineaAsync";
                pedido.FeModificacion = DateTimeOffset.UtcNow;

                linea.NoCantidad = request.NoCantidad;
                linea.MnSubtotal = request.NoCantidad * linea.MnPrecioUnitario - linea.MnDescuentoLinea;
                linea.ClOperadorModifica = userEmail;
                linea.NbArtefactoModifica = "PedidoService.ActualizarLineaAsync";
                linea.FeModificacion = DateTimeOffset.UtcNow;

                await RecalcularTotalesPedido(pedido);
                await dbContext.SaveChangesAsync();

                // Cargar relaciones para DTO
                await dbContext.Entry(sku).Reference(s => s.Variante).LoadAsync();
                if (sku.Variante != null)
                {
                    await dbContext.Entry(sku.Variante).Reference(v => v.Producto).LoadAsync();
                    await dbContext.Entry(sku.Variante).Reference(v => v.Combinacion).LoadAsync();
                }
                await dbContext.Entry(sku).Reference(s => s.Talla).LoadAsync();

                await transaction.CommitAsync();
                return MapToLineaDto(linea, sku);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    // ────────────────────────────────────────────────────────────────────────
    //  ELIMINAR LÍNEA
    // ────────────────────────────────────────────────────────────────────────
    public async Task<bool> EliminarLineaAsync(Guid idPedido, Guid idLinea, Guid idCliente)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                var pedido = await ObtenerPedidoConValidacion(idPedido, idCliente, "BORRADOR");

                var linea = await dbContext.LineasPedido
                    .FirstOrDefaultAsync(l => l.IdLineaPedido == idLinea && l.IdPedido == idPedido);

                if (linea == null) return false;

                // Bloquear SKU y liberar la reserva
                var sku = await dbContext.Skus
                    .FromSqlRaw("SELECT * FROM et_sku WITH (UPDLOCK, ROWLOCK) WHERE id_sku = {0}", linea.IdSku)
                    .FirstOrDefaultAsync();

                if (sku != null)
                {
                    sku.NoStockReservado = Math.Max(0, sku.NoStockReservado - linea.NoCantidad);
                    sku.FeModificacion = DateTimeOffset.UtcNow;
                }

                // Extender expiración
                pedido.FeExpiracion = DateTimeOffset.UtcNow.AddMinutes(20);
                pedido.FeModificacion = DateTimeOffset.UtcNow;

                dbContext.LineasPedido.Remove(linea);
                await RecalcularTotalesPedido(pedido);
                await dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    // ────────────────────────────────────────────────────────────────────────
    //  CONFIRMAR PEDIDO (BORRADOR → CONFIRMADO)
    // ────────────────────────────────────────────────────────────────────────
    public async Task<PedidoDetalleDto> ConfirmarPedidoAsync(
        Guid idPedido, Guid idCliente, Guid idUsuario, string userEmail)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                var pedido = await ObtenerPedidoConValidacion(idPedido, idCliente, "BORRADOR");

                // Validar expiración
                if (pedido.FeExpiracion.HasValue && DateTimeOffset.UtcNow > pedido.FeExpiracion.Value)
                    throw new InvalidOperationException("El pedido en borrador ha expirado. El inventario ya fue liberado.");

                // Validar que tenga al menos 1 línea
                var lineas = await dbContext.LineasPedido
                    .Where(l => l.IdPedido == idPedido)
                    .ToListAsync();

                if (lineas.Count == 0)
                    throw new InvalidOperationException("No se puede confirmar un pedido sin líneas.");

                // Transicionar estatus
                var estatusAnterior = await ResolverEstatusCatalogoAsync(pedido.ClEstatusPedido);
                pedido.ClEstatusPedido = "CONFIRMADO";
                pedido.FeExpiracion = null; // Limpiar expiración
                pedido.ClOperadorModifica = userEmail;
                pedido.NbArtefactoModifica = "PedidoService.ConfirmarPedidoAsync";
                pedido.FeModificacion = DateTimeOffset.UtcNow;

                var estatusNuevoCatalogo = await ResolverEstatusCatalogoAsync("CONFIRMADO")
                    ?? throw new InvalidOperationException("El estatus 'CONFIRMADO' no existe.");

                // Registrar historial
                dbContext.HistorialPedidos.Add(new etHistorialPedido
                {
                    Id = Guid.NewGuid(),
                    IdPedido = idPedido,
                    EstatusAnterior = estatusAnterior,
                    IdElemEstatusAnterior = estatusAnterior?.IdCatalogoElemento,
                    EstatusNuevo = estatusNuevoCatalogo,
                    IdElemEstatusNuevo = estatusNuevoCatalogo.IdCatalogoElemento,
                    IdUsuario = idUsuario,
                    Notas = "Pedido confirmado por el cliente",
                    RegistradoEn = DateTimeOffset.UtcNow
                });

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return await ObtenerPedidoDetalleAsync(idPedido)
                    ?? throw new InvalidOperationException("Error al confirmar el pedido.");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    // ────────────────────────────────────────────────────────────────────────
    //  CANCELAR BORRADOR (solo el dueño del pedido)
    // ────────────────────────────────────────────────────────────────────────
    public async Task<bool> CancelarBorradorAsync(Guid idPedido, Guid idCliente)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                var pedido = await ObtenerPedidoConValidacion(idPedido, idCliente, "BORRADOR");

                var lineas = await dbContext.LineasPedido
                    .Where(l => l.IdPedido == idPedido)
                    .ToListAsync();

                foreach (var linea in lineas)
                {
                    var sku = await dbContext.Skus
                        .FromSqlRaw("SELECT * FROM et_sku WITH (UPDLOCK, ROWLOCK) WHERE id_sku = {0}", linea.IdSku)
                        .FirstOrDefaultAsync();

                    if (sku != null)
                    {
                        sku.NoStockReservado = Math.Max(0, sku.NoStockReservado - linea.NoCantidad);
                        sku.FeModificacion = DateTimeOffset.UtcNow;
                    }
                }

                pedido.ClEstatusPedido = "CANCELADO";
                pedido.FeExpiracion = null;
                pedido.FeModificacion = DateTimeOffset.UtcNow;

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    // ────────────────────────────────────────────────────────────────────────
    //  CAMBIAR ESTATUS (Admin / Empleado asignado)
    // ────────────────────────────────────────────────────────────────────────
    public async Task<PedidoDetalleDto> CambiarEstatusAsync(
        Guid idPedido, CambiarEstatusRequest request, Guid idUsuario, string userEmail)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                var pedido = await dbContext.Pedidos
                    .FirstOrDefaultAsync(p => p.IdPedido == idPedido)
                    ?? throw new InvalidOperationException("Pedido no encontrado.");

                var estatusActual = pedido.ClEstatusPedido;
                var estatusNuevo = request.ClEstatusNuevo.ToUpperInvariant();

                // Validar transición
                if (!TransicionesValidas.TryGetValue(estatusActual, out var destinos) || !destinos.Contains(estatusNuevo))
                    throw new InvalidOperationException(
                        $"Transición inválida: '{estatusActual}' → '{estatusNuevo}'. " +
                        $"Transiciones permitidas: {string.Join(", ", TransicionesValidas.GetValueOrDefault(estatusActual, []))}");

                // Si se cancela o expira un pedido CONFIRMADO o BORRADOR, liberar stock reservado
                if ((estatusNuevo == "CANCELADO" || estatusNuevo == "EXPIRADO") && 
                    (estatusActual == "CONFIRMADO" || estatusActual == "BORRADOR"))
                {
                    var lineas = await dbContext.LineasPedido
                        .Where(l => l.IdPedido == idPedido)
                        .ToListAsync();

                    foreach (var linea in lineas)
                    {
                        var sku = await dbContext.Skus
                            .FromSqlRaw("SELECT * FROM et_sku WITH (UPDLOCK, ROWLOCK) WHERE id_sku = {0}", linea.IdSku)
                            .FirstOrDefaultAsync()
                            ?? throw new InvalidOperationException($"El SKU '{linea.IdSku}' no existe.");

                        sku.NoStockReservado = Math.Max(0, sku.NoStockReservado - linea.NoCantidad);
                        sku.ClOperadorModifica = userEmail;
                        sku.NbArtefactoModifica = "PedidoService.CambiarEstatusAsync.LiberarStock";
                        sku.FeModificacion = DateTimeOffset.UtcNow;
                    }
                }

                // Si deja de estar en BORRADOR, limpiar expiración
                if (estatusNuevo != "BORRADOR")
                {
                    pedido.FeExpiracion = null;
                }

                // Si se envía (FACTURADO → ENVIADO), descontar stock disponible y liberar reservado
                if (estatusNuevo == "ENVIADO" && estatusActual == "FACTURADO")
                {
                    var lineas = await dbContext.LineasPedido
                        .Where(l => l.IdPedido == idPedido)
                        .ToListAsync();

                    foreach (var linea in lineas)
                    {
                        var sku = await dbContext.Skus
                            .FromSqlRaw("SELECT * FROM et_sku WITH (UPDLOCK, ROWLOCK) WHERE id_sku = {0}", linea.IdSku)
                            .FirstOrDefaultAsync()
                            ?? throw new InvalidOperationException($"El SKU '{linea.IdSku}' no existe.");

                        sku.NoStockDisponible = Math.Max(0, sku.NoStockDisponible - linea.NoCantidad);
                        sku.NoStockReservado = Math.Max(0, sku.NoStockReservado - linea.NoCantidad);
                        sku.ClOperadorModifica = userEmail;
                        sku.NbArtefactoModifica = "PedidoService.CambiarEstatusAsync.DespacharStock";
                        sku.FeModificacion = DateTimeOffset.UtcNow;

                        // Registrar el movimiento de inventario de tipo VENTA
                        dbContext.MovimientosInventario.Add(new etMovimientoInventario
                        {
                            IdMovimiento = Guid.NewGuid(),
                            IdSku = sku.IdSku,
                            NoCantidad = -linea.NoCantidad,
                            ClTipoMovimiento = "VENTA",
                            DsMotivo = $"Despacho por Pedido #{idPedido}",
                            ClOperadorCrea = userEmail,
                            NbArtefactoCrea = "PedidoService.CambiarEstatusAsync.DespacharStock",
                            FeCreacion = DateTimeOffset.UtcNow
                        });
                    }
                }

                var estatusAnteriorCatalogo = await ResolverEstatusCatalogoAsync(estatusActual);
                pedido.ClEstatusPedido = estatusNuevo;
                pedido.ClOperadorModifica = userEmail;
                pedido.NbArtefactoModifica = "PedidoService.CambiarEstatusAsync";
                pedido.FeModificacion = DateTimeOffset.UtcNow;

                var estatusNuevoCatalogo = await ResolverEstatusCatalogoAsync(estatusNuevo)
                    ?? throw new InvalidOperationException($"El estatus '{estatusNuevo}' no existe.");

                dbContext.HistorialPedidos.Add(new etHistorialPedido
                {
                    Id = Guid.NewGuid(),
                    IdPedido = idPedido,
                    EstatusAnterior = estatusAnteriorCatalogo,
                    IdElemEstatusAnterior = estatusAnteriorCatalogo?.IdCatalogoElemento,
                    EstatusNuevo = estatusNuevoCatalogo,
                    IdElemEstatusNuevo = estatusNuevoCatalogo.IdCatalogoElemento,
                    IdUsuario = idUsuario,
                    Notas = request.Notas,
                    RegistradoEn = DateTimeOffset.UtcNow
                });

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return await ObtenerPedidoDetalleAsync(idPedido)
                    ?? throw new InvalidOperationException("Error al cambiar el estatus.");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    // ────────────────────────────────────────────────────────────────────────
    //  VERIFICAR ACCESO DE EMPLEADO AL PEDIDO
    // ────────────────────────────────────────────────────────────────────────
    public async Task<bool> EmpleadoTieneAccesoAlPedido(Guid idUsuario, Guid idPedido)
    {
        // Obtener el pedido para saber a qué cliente pertenece
        var pedido = await dbContext.Pedidos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdPedido == idPedido);
        if (pedido == null) return false;

        // Buscar si el usuario tiene un empleado vinculado
        var empleado = await dbContext.Empleados
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.IdUsuario == idUsuario && e.ClEstatusEmpleado == "ACTIVO");
        if (empleado == null) return false;

        // Verificar si el empleado está asignado al cliente del pedido
        return await dbContext.AsignacionesClienteEmpleado
            .AnyAsync(a => a.IdEmpleado == empleado.IdEmpleado
                        && a.IdCliente == pedido.IdCliente
                        && a.ClEstatusAsignacion == "ACTIVO");
    }

    // ────────────────────────────────────────────────────────────────────────
    //  CONSULTAS
    // ────────────────────────────────────────────────────────────────────────
    public async Task<IEnumerable<PedidoResumenDto>> ObtenerPedidosClienteAsync(Guid idCliente)
    {
        var pedidos = await dbContext.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.ClMoneda)
            .Include(p => p.Lineas)
                .ThenInclude(l => l.Sku)
                    .ThenInclude(s => s.Variante)
                        .ThenInclude(v => v.Producto)
            .Where(p => p.IdCliente == idCliente)
            .OrderByDescending(p => p.FeCreacion)
            .AsNoTracking()
            .ToListAsync();

        return pedidos.Select(p => {
            var subtotal = p.MnSubtotal == 0 && p.Lineas.Count > 0 ? p.Lineas.Sum(l => l.MnSubtotal) : p.MnSubtotal;
            var total = p.MnTotal == 0 && subtotal > 0 ? (subtotal - p.MnDescuentoComercial - p.MnDescuentoAdmin) : p.MnTotal;
            return new PedidoResumenDto(
                p.IdPedido,
                p.ClFolio,
                p.ClEstatusPedido,
                p.Cliente.NbComercial,
                p.ClMoneda?.NbCatalogoElemento,
                subtotal,
                p.MnDescuentoComercial,
                total,
                p.Lineas.Count,
                string.Join(", ", p.Lineas.Select(l => l.Sku.Variante?.Producto?.NbProducto).Where(n => !string.IsNullOrEmpty(n)).Distinct()),
                p.FePedido,
                p.FeCreacion
            );
        });
    }

    public async Task<PedidoDetalleDto?> ObtenerPedidoDetalleAsync(Guid idPedido)
    {
        var pedido = await dbContext.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.ClMoneda)
            .Include(p => p.DireccionEnvio)
            .Include(p => p.Politica)
            .Include(p => p.Lineas)
                .ThenInclude(l => l.Sku)
                    .ThenInclude(s => s.Variante)
                        .ThenInclude(v => v.Producto)
            .Include(p => p.Lineas)
                .ThenInclude(l => l.Sku)
                    .ThenInclude(s => s.Variante)
                        .ThenInclude(v => v.Combinacion)
            .Include(p => p.Lineas)
                .ThenInclude(l => l.Sku)
                    .ThenInclude(s => s.Talla)
            .Include(p => p.Historial)
                .ThenInclude(h => h.Usuario)
            .Include(p => p.Historial)
                .ThenInclude(h => h.EstatusNuevo)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdPedido == idPedido);

        if (pedido == null) return null;

        var subtotal = pedido.MnSubtotal == 0 && pedido.Lineas.Count > 0 ? pedido.Lineas.Sum(l => l.MnSubtotal) : pedido.MnSubtotal;
        var total = pedido.MnTotal == 0 && subtotal > 0 ? (subtotal - pedido.MnDescuentoComercial - pedido.MnDescuentoAdmin) : pedido.MnTotal;

        return new PedidoDetalleDto(
            pedido.IdPedido,
            pedido.ClFolio,
            pedido.ClEstatusPedido,
            pedido.IdCliente,
            pedido.Cliente.NbComercial,
            pedido.ClMoneda?.NbCatalogoElemento,
            pedido.IdDireccionEnvio,
            pedido.DireccionEnvio != null
                ? new DireccionEnvioDto(
                    pedido.DireccionEnvio.IdDireccion,
                    pedido.DireccionEnvio.NbAlias,
                    pedido.DireccionEnvio.DsLinea1,
                    pedido.DireccionEnvio.DsLinea2,
                    pedido.DireccionEnvio.NbCiudad,
                    pedido.DireccionEnvio.NbEstado,
                    pedido.DireccionEnvio.ClCodigoPostal,
                    pedido.DireccionEnvio.ClPais)
                : null,
            pedido.Politica?.NbPolitica,
            subtotal,
            pedido.MnDescuentoComercial,
            pedido.MnDescuentoAdmin,
            total,
            pedido.FePedido,
            pedido.Lineas.Select(l => new LineaPedidoDto(
                l.IdLineaPedido,
                l.IdSku,
                l.Sku.ClItem,
                l.Sku.Variante?.Producto?.NbProducto,
                l.Sku.Variante?.Combinacion?.NbCatalogoElemento,
                l.Sku.Talla?.NbCatalogoElemento,
                l.NoCantidad,
                l.MnPrecioUnitario,
                l.MnDescuentoLinea,
                l.MnSubtotal
            )).ToList(),
            pedido.Historial
                .OrderBy(h => h.RegistradoEn)
                .Select(h => new HistorialPedidoDto(
                    h.EstatusAnterior?.NbCatalogoElemento,
                    h.EstatusNuevo.NbCatalogoElemento,
                    h.Usuario.Email ?? "Sistema",
                    h.Notas,
                    h.RegistradoEn
                )).ToList()
        );
    }

    public async Task<IEnumerable<PedidoResumenDto>> ObtenerTodosPedidosAsync(
        string? filtroEstatus, Guid? filtroCliente, Guid? filtroEmpleado, Guid idUsuarioSolicitante, bool esAdmin)
    {
        var query = dbContext.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.ClMoneda)
            .Include(p => p.Lineas)
                .ThenInclude(l => l.Sku)
                    .ThenInclude(s => s.Variante)
                        .ThenInclude(v => v.Producto)
            .AsQueryable();

        // ── SEGURIDAD: Filtrar por cartera de empleado si no es Admin ──
        if (!esAdmin)
        {
            var empleado = await dbContext.Empleados
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdUsuario == idUsuarioSolicitante && e.ClEstatusEmpleado == "ACTIVO");

            if (empleado == null)
            {
                // Si no es admin y tampoco es un empleado activo válido, no puede ver nada
                return [];
            }

            var clienteIdsAsignados = await dbContext.AsignacionesClienteEmpleado
                .Where(a => a.IdEmpleado == empleado.IdEmpleado && a.ClEstatusAsignacion == "ACTIVO")
                .Select(a => a.IdCliente)
                .ToListAsync();

            query = query.Where(p => clienteIdsAsignados.Contains(p.IdCliente));
        }
        else
        {
            // El admin puede filtrar opcionalmente por un empleado asignado en particular
            if (filtroEmpleado.HasValue)
            {
                var clienteIdsAsignados = await dbContext.AsignacionesClienteEmpleado
                    .Where(a => a.IdEmpleado == filtroEmpleado.Value && a.ClEstatusAsignacion == "ACTIVO")
                    .Select(a => a.IdCliente)
                    .ToListAsync();

                query = query.Where(p => clienteIdsAsignados.Contains(p.IdCliente));
            }
        }

        if (!string.IsNullOrWhiteSpace(filtroEstatus))
            query = query.Where(p => p.ClEstatusPedido == filtroEstatus.ToUpperInvariant());

        if (filtroCliente.HasValue)
            query = query.Where(p => p.IdCliente == filtroCliente.Value);

        var pedidos = await query
            .OrderByDescending(p => p.FeCreacion)
            .AsNoTracking()
            .ToListAsync();

        return pedidos.Select(p => {
            var subtotal = p.MnSubtotal == 0 && p.Lineas.Count > 0 ? p.Lineas.Sum(l => l.MnSubtotal) : p.MnSubtotal;
            var total = p.MnTotal == 0 && subtotal > 0 ? (subtotal - p.MnDescuentoComercial - p.MnDescuentoAdmin) : p.MnTotal;
            return new PedidoResumenDto(
                p.IdPedido,
                p.ClFolio,
                p.ClEstatusPedido,
                p.Cliente.NbComercial,
                p.ClMoneda?.NbCatalogoElemento,
                subtotal,
                p.MnDescuentoComercial,
                total,
                p.Lineas.Count,
                string.Join(", ", p.Lineas.Select(l => l.Sku.Variante?.Producto?.NbProducto).Where(n => !string.IsNullOrEmpty(n)).Distinct()),
                p.FePedido,
                p.FeCreacion
            );
        });
    }

    public async Task<DashboardResumenDto> ObtenerDashboardResumenAsync()
    {
        var totalBorrador = await dbContext.Pedidos.CountAsync(p => p.ClEstatusPedido == "BORRADOR");
        var totalConfirmados = await dbContext.Pedidos.CountAsync(p => p.ClEstatusPedido == "CONFIRMADO");
        var totalFacturados = await dbContext.Pedidos.CountAsync(p => p.ClEstatusPedido == "FACTURADO");
        var totalEnviados = await dbContext.Pedidos.CountAsync(p => p.ClEstatusPedido == "ENVIADO");
        var totalCancelados = await dbContext.Pedidos.CountAsync(p => p.ClEstatusPedido == "CANCELADO");

        var montoPendienteFacturar = await dbContext.Pedidos
            .Where(p => p.ClEstatusPedido == "CONFIRMADO")
            .SumAsync(p => p.MnTotal);

        var montoTotalFacturado = await dbContext.Pedidos
            .Where(p => p.ClEstatusPedido == "FACTURADO")
            .SumAsync(p => p.MnTotal);

        var ultimosPedidos = await dbContext.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.ClMoneda)
            .Include(p => p.Lineas)
                .ThenInclude(l => l.Sku)
                    .ThenInclude(s => s.Variante)
                        .ThenInclude(v => v.Producto)
            .Where(p => p.ClEstatusPedido == "CONFIRMADO")
            .OrderByDescending(p => p.FeCreacion)
            .Take(5)
            .AsNoTracking()
            .ToListAsync();

        var mappedUltimosPedidos = ultimosPedidos.Select(p => new PedidoResumenDto(
            p.IdPedido,
            p.ClFolio,
            p.ClEstatusPedido,
            p.Cliente.NbComercial,
            p.ClMoneda?.NbCatalogoElemento,
            p.MnSubtotal,
            p.MnDescuentoComercial,
            p.MnTotal,
            p.Lineas.Count,
            string.Join(", ", p.Lineas.Select(l => l.Sku.Variante?.Producto?.NbProducto).Where(n => !string.IsNullOrEmpty(n)).Distinct()),
            p.FePedido,
            p.FeCreacion
        )).ToList();

        return new DashboardResumenDto(
            totalBorrador,
            totalConfirmados,
            totalFacturados,
            totalEnviados,
            totalCancelados,
            montoPendienteFacturar,
            montoTotalFacturado,
            mappedUltimosPedidos
        );
    }

    // ════════════════════════════════════════════════════════════════════════
    //  MÉTODOS AUXILIARES PRIVADOS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Genera folio secuencial por día: PED-YYYYMMDD-0001
    /// </summary>
    private async Task<string> GenerarFolioAsync()
    {
        var hoy = DateTimeOffset.UtcNow;
        var inicioDelDia = new DateTimeOffset(hoy.Year, hoy.Month, hoy.Day, 0, 0, 0, TimeSpan.Zero);
        var finDelDia = inicioDelDia.AddDays(1);

        var count = await dbContext.Pedidos
            .CountAsync(p => p.FeCreacion >= inicioDelDia && p.FeCreacion < finDelDia);

        return $"PED-{hoy:yyyyMMdd}-{(count + 1):D4}";
    }

    /// <summary>
    /// Obtiene un pedido validando ownership (el cliente es el dueño) y estatus esperado.
    /// Fundamental para seguridad: un cliente jamás puede tocar el pedido de otro.
    /// </summary>
    private async Task<etPedido> ObtenerPedidoConValidacion(Guid idPedido, Guid idCliente, string estatusRequerido)
    {
        var pedido = await dbContext.Pedidos
            .FirstOrDefaultAsync(p => p.IdPedido == idPedido)
            ?? throw new InvalidOperationException("Pedido no encontrado.");

        // ── SEGURIDAD: ownership check ──
        if (pedido.IdCliente != idCliente)
            throw new UnauthorizedAccessException("No tienes acceso a este pedido.");

        if (pedido.ClEstatusPedido != estatusRequerido)
            throw new InvalidOperationException(
                $"Esta operación solo está permitida en pedidos con estatus '{estatusRequerido}'. " +
                $"Estatus actual: '{pedido.ClEstatusPedido}'.");

        return pedido;
    }

    /// <summary>
    /// Recalcula MnSubtotal, MnDescuentoComercial y MnTotal del pedido
    /// basándose en todas sus líneas actuales.
    /// </summary>
    private async Task RecalcularTotalesPedido(etPedido pedido)
    {
        var subtotal = await dbContext.LineasPedido
            .Where(l => l.IdPedido == pedido.IdPedido)
            .SumAsync(l => l.MnSubtotal);

        pedido.MnSubtotal = subtotal;

        // Aplicar descuento comercial si hay política asignada
        if (pedido.IdPolitica.HasValue)
        {
            var politica = await dbContext.PoliticasPrecios
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdPolitica == pedido.IdPolitica.Value);

            if (politica != null)
                pedido.MnDescuentoComercial = subtotal * politica.MnFactorDescuento;
        }

        pedido.MnTotal = pedido.MnSubtotal - pedido.MnDescuentoComercial - pedido.MnDescuentoAdmin;
    }

    /// <summary>
    /// Resuelve un nombre de estatus ("BORRADOR") al elemento del catálogo CCatalogoElemento.
    /// Necesario porque etHistorialPedido usa EstatusPedido (que es CCatalogoElemento).
    /// </summary>
    private async Task<CCatalogoElemento?> ResolverEstatusCatalogoAsync(string nombreEstatus)
    {
        return await dbContext.CCatalogoElementos
            .FirstOrDefaultAsync(e =>
                e.Catalogo!.ClCatalogo == "ESTATUS_PEDIDO" &&
                e.ClCatalogoElemento == nombreEstatus);
    }

    private static LineaPedidoDto MapToLineaDto(etLineaPedido linea, etSku sku)
    {
        return new LineaPedidoDto(
            linea.IdLineaPedido,
            linea.IdSku,
            sku.ClItem,
            sku.Variante?.Producto?.NbProducto,
            sku.Variante?.Combinacion?.NbCatalogoElemento,
            sku.Talla?.NbCatalogoElemento,
            linea.NoCantidad,
            linea.MnPrecioUnitario,
            linea.MnDescuentoLinea,
            linea.MnSubtotal
        );
    }

    public async Task ForzarExpiracionAsync(Guid idPedido)
    {
        var pedido = await dbContext.Pedidos.FindAsync(idPedido)
            ?? throw new InvalidOperationException("Pedido no encontrado.");
        pedido.FeExpiracion = DateTimeOffset.UtcNow.AddMinutes(-5);
        await dbContext.SaveChangesAsync();
    }
}
