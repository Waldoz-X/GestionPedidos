using GestionPedidos.Contracts.Precios;
using GestionPedidos.Data;
using GestionPedidos.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPedidos.Services;

// ════════════════════════════════════════════════════════════════════════════
//  INTERFAZ
// ════════════════════════════════════════════════════════════════════════════
public interface IPrecioService
{
    /// <summary>Resuelve el precio final para el SKU del cliente que está en el JWT.</summary>
    Task<PrecioResueltoDto> ResolverPrecioAsync(Guid idSku, Guid idCliente);

    /// <summary>Resuelve precios de múltiples SKUs de una vez (para cargar catálogo completo).</summary>
    Task<List<PrecioResueltoDto>> ResolverBulkAsync(List<Guid> idSkus, Guid idCliente);

    /// <summary>Lista todos los precios (solo Admin).</summary>
    Task<IEnumerable<PrecioDto>> ObtenerTodosAsync();

    /// <summary>Crea o actualiza un precio y registra el historial de cambio.</summary>
    Task<PrecioDto> CrearOActualizarAsync(PrecioUpsertDto dto, string userEmail, Guid userId);

    /// <summary>Carga masiva de precios desde JSON (Python/Excel). Traduce claves a IDs en memoria.</summary>
    Task<PrecioBulkResultDto> CargarMasivaAsync(IEnumerable<PrecioBulkDto> dtos, string userEmail, Guid userId);

    /// <summary>Historial inmutable de cambios de un precio.</summary>
    Task<IEnumerable<HistorialPrecioDto>> ObtenerHistorialAsync(Guid idPrecio);
}

// ════════════════════════════════════════════════════════════════════════════
//  IMPLEMENTACIÓN
// ════════════════════════════════════════════════════════════════════════════
public class PrecioService(AppDbContext dbContext) : IPrecioService
{
    // ────────────────────────────────────────────────────────────────────────
    //  MOTOR DE PRECIOS EN CASCADA (2 niveles)
    // ────────────────────────────────────────────────────────────────────────

    public async Task<PrecioResueltoDto> ResolverPrecioAsync(Guid idSku, Guid idCliente)
    {
        var ahora = DateTimeOffset.UtcNow;

        // ── NIVEL 1: Precio directo del cliente ──────────────────────────────
        var precioDirecto = await dbContext.Precios
            .Where(p =>
                p.IdSku == idSku &&
                p.IdCliente == idCliente &&
                p.ClEstatusPrecio == "ACTIVO" &&
                p.FeVigenteDesde <= ahora &&
                (p.FeVigenteHasta == null || p.FeVigenteHasta >= ahora))
            .FirstOrDefaultAsync();

        if (precioDirecto != null)
        {
            return new PrecioResueltoDto(
                IdSku: idSku,
                PrecioEncontrado: true,
                PrecioFinal: precioDirecto.MnPrecioNeto,
                Moneda: precioDirecto.ClMoneda.ToString(),
                NivelAplicado: 1,
                NbPoliticaAplicada: null,
                VigenteHasta: precioDirecto.FeVigenteHasta,
                Mensaje: null
            );
        }

        // ── NIVEL 2: Precio por política del cliente ─────────────────────────
        // Obtenemos los IDs de todas las políticas asignadas al cliente
        var idsPoliticasCliente = await dbContext.ClientesPoliticas
            .Where(cp => cp.IdCliente == idCliente)
            .Select(cp => cp.IdPolitica)
            .ToListAsync();

        if (idsPoliticasCliente.Count > 0)
        {
            var precioPolitica = await dbContext.Precios
                .Include(p => p.Politica)
                .Where(p =>
                    p.IdSku == idSku &&
                    p.IdCliente == null &&
                    p.IdPolitica != null &&
                    idsPoliticasCliente.Contains(p.IdPolitica!.Value) &&
                    p.ClEstatusPrecio == "ACTIVO" &&
                    p.FeVigenteDesde <= ahora &&
                    (p.FeVigenteHasta == null || p.FeVigenteHasta >= ahora))
                // Tomamos la política con mayor prioridad (número más alto = mejor precio)
                .OrderByDescending(p => p.Politica!.NoPrioridad)
                .FirstOrDefaultAsync();

            if (precioPolitica != null)
            {
                return new PrecioResueltoDto(
                    IdSku: idSku,
                    PrecioEncontrado: true,
                    PrecioFinal: precioPolitica.MnPrecioNeto,
                    Moneda: precioPolitica.ClMoneda.ToString(),
                    NivelAplicado: 2,
                    NbPoliticaAplicada: precioPolitica.Politica?.NbPolitica,
                    VigenteHasta: precioPolitica.FeVigenteHasta,
                    Mensaje: null
                );
            }
        }

        // ── SIN PRECIO CONFIGURADO ───────────────────────────────────────────
        return new PrecioResueltoDto(
            IdSku: idSku,
            PrecioEncontrado: false,
            PrecioFinal: null,
            Moneda: null,
            NivelAplicado: 0,
            NbPoliticaAplicada: null,
            VigenteHasta: null,
            Mensaje: "No existe precio configurado para este SKU y cliente. Contacte al administrador."
        );
    }

    public async Task<List<PrecioResueltoDto>> ResolverBulkAsync(List<Guid> idSkus, Guid idCliente)
    {
        // Ejecutamos en paralelo para mayor velocidad
        var tareas = idSkus.Select(idSku => ResolverPrecioAsync(idSku, idCliente));
        var resultados = await Task.WhenAll(tareas);
        return [.. resultados];
    }

    // ────────────────────────────────────────────────────────────────────────
    //  CRUD (SOLO ADMIN)
    // ────────────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<PrecioDto>> ObtenerTodosAsync()
    {
        return await dbContext.Precios
            .Include(p => p.Sku)
            .Include(p => p.Cliente)
            .Include(p => p.Politica)
            .AsNoTracking()
            .Select(p => new PrecioDto(
                p.IdPrecio,
                p.IdSku,
                p.Sku.ClItem,
                p.IdCliente,
                p.Cliente != null ? p.Cliente.NbComercial : null,
                p.IdPolitica,
                p.Politica != null ? p.Politica.NbPolitica : null,
                p.MnPrecioNeto,
                p.ClMoneda.ToString(),
                p.ClEstatusPrecio,
                p.FeVigenteDesde,
                p.FeVigenteHasta
            ))
            .ToListAsync();
    }

    public async Task<PrecioDto> CrearOActualizarAsync(PrecioUpsertDto dto, string userEmail, Guid userId)
    {
        // Buscar si ya existe un precio para este SKU + cliente o política (para hacer upsert)
        var precioExistente = await dbContext.Precios
            .FirstOrDefaultAsync(p =>
                p.IdSku == dto.IdSku &&
                p.IdCliente == dto.IdCliente &&
                p.IdPolitica == dto.IdPolitica);

        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                if (precioExistente != null)
                {
                    // ── UPDATE + registrar historial ──
                    var historial = new etHistorialPrecio
                    {
                        Id = Guid.NewGuid(),
                        IdPrecio = precioExistente.IdPrecio,
                        PrecioAnterior = precioExistente.MnPrecioNeto,
                        PrecioNuevo = dto.MnPrecioNeto,
                        IdUsuario = userId,
                        RegistradoEn = DateTimeOffset.UtcNow
                    };
                    dbContext.HistorialPrecios.Add(historial);

                    precioExistente.MnPrecioNeto = dto.MnPrecioNeto;
                    precioExistente.ClMoneda = dto.ClMoneda.ToUpperInvariant();
                    precioExistente.ClEstatusPrecio = "ACTIVO";
                    precioExistente.FeVigenteDesde = dto.FeVigenteDesde;
                    precioExistente.FeVigenteHasta = dto.FeVigenteHasta;
                    precioExistente.ClOperadorModifica = userEmail;
                    precioExistente.NbArtefactoModifica = "PrecioService.CrearOActualizarAsync";
                    precioExistente.FeModificacion = DateTimeOffset.UtcNow;
                }
                else
                {
                    // ── INSERT ──
                    precioExistente = new etPrecio
                    {
                        IdPrecio = Guid.NewGuid(),
                        IdSku = dto.IdSku,
                        IdCliente = dto.IdCliente,
                        IdPolitica = dto.IdPolitica,
                        MnPrecioNeto = dto.MnPrecioNeto,
                        ClMoneda = dto.ClMoneda.ToUpperInvariant(),
                        FeVigenteDesde = dto.FeVigenteDesde,
                        FeVigenteHasta = dto.FeVigenteHasta,
                        ClEstatusPrecio = "ACTIVO",
                        ClOperadorCrea = userEmail,
                        NbArtefactoCrea = "PrecioService.CrearOActualizarAsync",
                        FeCreacion = DateTimeOffset.UtcNow
                    };
                    dbContext.Precios.Add(precioExistente);
                }

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            // Recargar con relaciones para el DTO de respuesta
            var precioGuardado = await dbContext.Precios
                .Include(p => p.Sku)
                .Include(p => p.Cliente)
                .Include(p => p.Politica)
                .FirstAsync(p => p.IdPrecio == precioExistente.IdPrecio);

            return new PrecioDto(
                precioGuardado.IdPrecio,
                precioGuardado.IdSku,
                precioGuardado.Sku.ClItem,
                precioGuardado.IdCliente,
                precioGuardado.Cliente?.NbComercial,
                precioGuardado.IdPolitica,
                precioGuardado.Politica?.NbPolitica,
                precioGuardado.MnPrecioNeto,
                precioGuardado.ClMoneda.ToString(),
                precioGuardado.ClEstatusPrecio,
                precioGuardado.FeVigenteDesde,
                precioGuardado.FeVigenteHasta
            );
        });
    }

    // ────────────────────────────────────────────────────────────────────────
    //  CARGA MASIVA (Python/Excel)
    // ────────────────────────────────────────────────────────────────────────

    public async Task<PrecioBulkResultDto> CargarMasivaAsync(IEnumerable<PrecioBulkDto> dtos, string userEmail, Guid userId)
    {
        // ── PASO 1: Pre-cargar tablas de lookup a memoria (1 query c/u) ──────
        var skusPorClave = await dbContext.Skus
            .AsNoTracking()
            .ToDictionaryAsync(s => s.ClItem ?? string.Empty, s => s.IdSku);

        var skusPorBarras = await dbContext.Skus
            .Where(s => s.ClCodigoBarras != null)
            .AsNoTracking()
            .ToDictionaryAsync(s => s.ClCodigoBarras!, s => s.IdSku);

        var clientesPorNombre = await dbContext.Clientes
            .AsNoTracking()
            .ToDictionaryAsync(c => c.NbComercial, c => c.IdCliente, StringComparer.OrdinalIgnoreCase);

        var politicasPorNombre = await dbContext.PoliticasPrecios
            .AsNoTracking()
            .ToDictionaryAsync(p => p.NbPolitica, p => p.IdPolitica, StringComparer.OrdinalIgnoreCase);

        // ── PASO 2: Iterar y construir objetos en memoria ────────────────────
        var errores = new List<string>();
        var preciosParaInsertar = new List<etPrecio>();
        var preciosParaActualizar = new List<(etPrecio Existente, decimal NuevoPrecio, string NuevaMoneda)>();
        int fila = 0;

        // Pre-cargar precios existentes para detectar upsert
        var preciosExistentes = await dbContext.Precios.ToListAsync();

        foreach (var dto in dtos)
        {
            fila++;

            // Resolver SKU (por ClItem primero, luego por código de barras)
            Guid idSku;
            if (!skusPorClave.TryGetValue(dto.ClItemOCodigoBarras, out idSku) &&
                !skusPorBarras.TryGetValue(dto.ClItemOCodigoBarras, out idSku))
            {
                errores.Add($"Fila {fila}: SKU '{dto.ClItemOCodigoBarras}' no encontrado (ni como ClItem ni código de barras).");
                continue;
            }

            // Resolver Cliente (opcional)
            Guid? idCliente = null;
            if (!string.IsNullOrWhiteSpace(dto.NbComercialCliente))
            {
                if (!clientesPorNombre.TryGetValue(dto.NbComercialCliente, out var idCli))
                {
                    errores.Add($"Fila {fila}: Cliente '{dto.NbComercialCliente}' no encontrado.");
                    continue;
                }
                idCliente = idCli;
            }

            // Resolver Política (opcional)
            Guid? idPolitica = null;
            if (!string.IsNullOrWhiteSpace(dto.NbPolitica))
            {
                if (!politicasPorNombre.TryGetValue(dto.NbPolitica, out var idPol))
                {
                    errores.Add($"Fila {fila}: Política '{dto.NbPolitica}' no encontrada.");
                    continue;
                }
                idPolitica = idPol;
            }

            // Validar moneda buscando en el catálogo de monedas (ya cargado en memoria)
            var monedaElemento = await dbContext.CCatalogoElementos
                .Include(e => e.Catalogo)
                .AsNoTracking()
                .FirstOrDefaultAsync(e =>
                    e.Catalogo.ClCatalogo == "MONEDAS" &&
                    e.ClCatalogoElemento == dto.ClMoneda.ToUpperInvariant());

            if (monedaElemento == null)
            {
                errores.Add($"Fila {fila}: Moneda '{dto.ClMoneda}' no válida. Use las claves de catálogo: MXN, USD, EUR, etc.");
                continue;
            }

            // Parsear fecha (opcional)
            DateTimeOffset? feVigenteHasta = null;
            if (!string.IsNullOrWhiteSpace(dto.FeVigenteHasta))
            {
                if (!DateTimeOffset.TryParse(dto.FeVigenteHasta, out var fecha))
                {
                    errores.Add($"Fila {fila}: Fecha '{dto.FeVigenteHasta}' no es válida. Use formato yyyy-MM-dd.");
                    continue;
                }
                feVigenteHasta = fecha;
            }

            // Detectar si es INSERT o UPDATE
            var existente = preciosExistentes.FirstOrDefault(p =>
                p.IdSku == idSku &&
                p.IdCliente == idCliente &&
                p.IdPolitica == idPolitica);

            if (existente != null)
            {
                preciosParaActualizar.Add((existente, dto.MnPrecioNeto, monedaElemento.ClCatalogoElemento));
            }
            else
            {
                preciosParaInsertar.Add(new etPrecio
                {
                    IdPrecio = Guid.NewGuid(),
                    IdSku = idSku,
                    IdCliente = idCliente,
                    IdPolitica = idPolitica,
                    MnPrecioNeto = dto.MnPrecioNeto,
                    ClMoneda = monedaElemento.ClCatalogoElemento,
                    FeVigenteDesde = DateTimeOffset.UtcNow,
                    FeVigenteHasta = feVigenteHasta,
                    ClEstatusPrecio = "ACTIVO",
                    ClOperadorCrea = userEmail,
                    NbArtefactoCrea = "PrecioService.CargarMasivaAsync",
                    FeCreacion = DateTimeOffset.UtcNow
                });
            }
        }

        // Si hay errores de mapeo, no guardamos nada (todo o nada)
        if (errores.Count > 0)
        {
            return new PrecioBulkResultDto(fila, 0, 0, errores);
        }

        // ── PASO 3: Guardar todo en una sola transacción atómica ─────────────
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                // Inserts
                if (preciosParaInsertar.Count > 0)
                    dbContext.Precios.AddRange(preciosParaInsertar);

                // Updates + historial
                var historialBulk = new List<etHistorialPrecio>();
                foreach (var (existente, nuevoPrecio, nuevaMoneda) in preciosParaActualizar)
                {
                    historialBulk.Add(new etHistorialPrecio
                    {
                        Id = Guid.NewGuid(),
                        IdPrecio = existente.IdPrecio,
                        PrecioAnterior = existente.MnPrecioNeto,
                        PrecioNuevo = nuevoPrecio,
                        IdUsuario = userId,
                        RegistradoEn = DateTimeOffset.UtcNow
                    });
                    existente.MnPrecioNeto = nuevoPrecio;
                    existente.ClMoneda = nuevaMoneda;  // string "MXN", "EUR", etc.
                    existente.ClOperadorModifica = userEmail;
                    existente.NbArtefactoModifica = "PrecioService.CargarMasivaAsync";
                    existente.FeModificacion = DateTimeOffset.UtcNow;
                }

                if (historialBulk.Count > 0)
                    dbContext.HistorialPrecios.AddRange(historialBulk);

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return new PrecioBulkResultDto(
                TotalRecibidos: fila,
                Insertados: preciosParaInsertar.Count,
                Actualizados: preciosParaActualizar.Count,
                Errores: []
            );
        });
    }

    // ────────────────────────────────────────────────────────────────────────
    //  HISTORIAL
    // ────────────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<HistorialPrecioDto>> ObtenerHistorialAsync(Guid idPrecio)
    {
        return await dbContext.HistorialPrecios
            .Include(h => h.Usuario)
            .Where(h => h.IdPrecio == idPrecio)
            .OrderByDescending(h => h.RegistradoEn)
            .AsNoTracking()
            .Select(h => new HistorialPrecioDto(
                h.Id,
                h.PrecioAnterior,
                h.PrecioNuevo,
                h.Usuario.UserName ?? h.Usuario.Email ?? "Desconocido",
                h.RegistradoEn
            ))
            .ToListAsync();
    }
}
