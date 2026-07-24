using GestionPedidos.Contracts.Visibilidad;
using GestionPedidos.Data;
using GestionPedidos.Models;
using GestionPedidos.Models.Catalogo;
using Microsoft.EntityFrameworkCore;

namespace GestionPedidos.Services;

// ════════════════════════════════════════════════════════════════════════════
//  INTERFAZ
// ════════════════════════════════════════════════════════════════════════════
public interface IVisibilidadService
{
    /// <summary>
    /// Whitelist: devuelve SOLO los productos que tienen asignación VISIBLE o EXCLUSIVO
    /// para el cliente indicado. Si no hay registro en et_visibilidad_catalogo, no se ve.
    /// </summary>
    Task<IEnumerable<ProductoVisibleDto>> ObtenerProductosVisiblesAsync(Guid idCliente);

    /// <summary>Asigna o actualiza visibilidad de un producto a un cliente.</summary>
    Task<VisibilidadDto> AsignarVisibilidadAsync(VisibilidadUpsertDto dto, string userEmail);

    /// <summary>Revoca el acceso de un cliente a un producto (elimina el registro del ACL).</summary>
    Task<bool> RevocarVisibilidadAsync(Guid idCliente, Guid idProducto);

    /// <summary>Revoca una regla de visibilidad por su ID único.</summary>
    Task<bool> RevocarVisibilidadPorIdAsync(Guid idVisibilidad);

    /// <summary>Lista todos los clientes que tienen acceso a un producto dado.</summary>
    Task<IEnumerable<VisibilidadDto>> ObtenerClientesDeProductoAsync(Guid idProducto);

    /// <summary>Asigna visibilidad en bulk: 1 cliente + N productos en una sola transacción.</summary>
    Task<VisibilidadBulkResponse> AsignarVisibilidadBulkAsync(VisibilidadBulkRequest request, string userEmail);

    /// <summary>Evalúa si un cliente tiene acceso a un SKU específico utilizando la jerarquía SKU -> Variante -> Producto -> Exclusividad global.</summary>
    Task<bool> TieneAccesoASkuAsync(Guid idCliente, Guid idProducto, Guid idVariante, Guid idSku);

    /// <summary>Evalúa masivamente el acceso a múltiples SKUs para un cliente (optimizado para catálogos).</summary>
    Task<Dictionary<Guid, bool>> EvaluarAccesoSkusMasivoAsync(Guid idCliente, IEnumerable<Guid> productoIds, IEnumerable<Guid> varianteIds, IEnumerable<Guid> skuIds);
}

// ════════════════════════════════════════════════════════════════════════════
//  IMPLEMENTACIÓN
// ════════════════════════════════════════════════════════════════════════════
public class VisibilidadService(AppDbContext dbContext) : IVisibilidadService
{
    public async Task<IEnumerable<ProductoVisibleDto>> ObtenerProductosVisiblesAsync(Guid idCliente)
    {
        // Whitelist pura: solo productos con registro explícito VISIBLE o EXCLUSIVO
        // El cliente jamás ve productos con ClTipoAcceso == "OCULTO" ni productos sin registro.
        return await dbContext.VisibilidadesCatalogo
            .Include(v => v.Producto)
                .ThenInclude(p => p!.Categoria)
            .Include(v => v.Producto)
                .ThenInclude(p => p!.LineaColeccion)
            .Where(v =>
                v.IdCliente == idCliente &&
                v.ClEstatusVisibilidad == "ACTIVO" &&
                (v.ClTipoAcceso == "VISIBLE" || v.ClTipoAcceso == "EXCLUSIVO") &&
                v.Producto != null && v.Producto.ClEstatusProducto == "ACTIVO")
            .AsNoTracking()
            .Select(v => new ProductoVisibleDto(
                v.Producto!.IdProducto,
                v.Producto!.ClProducto,
                v.Producto!.NbProducto,
                v.Producto!.Categoria.NbCatalogoElemento,
                v.Producto!.LineaColeccion != null ? v.Producto!.LineaColeccion.NbCatalogoElemento : null,
                v.ClTipoAcceso
            ))
            .ToListAsync();
    }

    public async Task<VisibilidadDto> AsignarVisibilidadAsync(VisibilidadUpsertDto dto, string userEmail)
    {
        Guid? idProducto = dto.IdProducto;
        Guid? idVariante = dto.IdVariante;
        Guid? idSku = dto.IdSku;

        // Auto-resolver relaciones jerárquicas superiores
        if (idSku.HasValue && (!idVariante.HasValue || !idProducto.HasValue))
        {
            var sku = await dbContext.Skus
                .Include(s => s.Variante)
                .FirstOrDefaultAsync(s => s.IdSku == idSku.Value);
            if (sku != null)
            {
                idVariante ??= sku.IdVariante;
                idProducto ??= sku.Variante?.IdProducto;
            }
        }
        else if (idVariante.HasValue && !idProducto.HasValue)
        {
            var variante = await dbContext.Variantes
                .FirstOrDefaultAsync(v => v.IdVariante == idVariante.Value);
            if (variante != null)
            {
                idProducto ??= variante.IdProducto;
            }
        }

        // 1. Validar redundancia con reglas de nivel superior
        if (idSku.HasValue)
        {
            var reglaSuperiorRedundante = await dbContext.VisibilidadesCatalogo.AnyAsync(v =>
                v.IdCliente == dto.IdCliente &&
                v.ClEstatusVisibilidad == "ACTIVO" &&
                v.ClTipoAcceso == dto.ClTipoAcceso &&
                ((v.IdVariante == idVariante && v.IdSku == null) || (v.IdProducto == idProducto && v.IdVariante == null && v.IdSku == null)));

            if (reglaSuperiorRedundante)
            {
                throw new InvalidOperationException("Esta regla a nivel de SKU es redundante porque ya está cubierta por una regla superior con el mismo tipo de acceso.");
            }
        }
        else if (idVariante.HasValue)
        {
            var reglaSuperiorRedundante = await dbContext.VisibilidadesCatalogo.AnyAsync(v =>
                v.IdCliente == dto.IdCliente &&
                v.ClEstatusVisibilidad == "ACTIVO" &&
                v.ClTipoAcceso == dto.ClTipoAcceso &&
                v.IdProducto == idProducto && v.IdVariante == null && v.IdSku == null);

            if (reglaSuperiorRedundante)
            {
                throw new InvalidOperationException("Esta regla a nivel de Variante es redundante porque ya está cubierta por una regla superior con el mismo tipo de acceso.");
            }
        }

        // 2. Limpieza de reglas inferiores que se vuelven redundantes
        if (idProducto.HasValue && !idVariante.HasValue && !idSku.HasValue)
        {
            var hijasRedundantes = await dbContext.VisibilidadesCatalogo
                .Where(v => v.IdCliente == dto.IdCliente &&
                            v.IdProducto == idProducto &&
                            (v.IdVariante != null || v.IdSku != null) &&
                            v.ClTipoAcceso == dto.ClTipoAcceso)
                .ToListAsync();

            if (hijasRedundantes.Any())
            {
                dbContext.VisibilidadesCatalogo.RemoveRange(hijasRedundantes);
            }
        }
        else if (idVariante.HasValue && !idSku.HasValue)
        {
            var hijasRedundantes = await dbContext.VisibilidadesCatalogo
                .Where(v => v.IdCliente == dto.IdCliente &&
                            v.IdVariante == idVariante &&
                            v.IdSku != null &&
                            v.ClTipoAcceso == dto.ClTipoAcceso)
                .ToListAsync();

            if (hijasRedundantes.Any())
            {
                dbContext.VisibilidadesCatalogo.RemoveRange(hijasRedundantes);
            }
        }

        var existente = await dbContext.VisibilidadesCatalogo
            .FirstOrDefaultAsync(v => v.IdCliente == dto.IdCliente 
                                   && v.IdProducto == idProducto
                                   && v.IdVariante == idVariante
                                   && v.IdSku == idSku);

        if (existente != null)
        {
            // Update
            existente.ClTipoAcceso = dto.ClTipoAcceso;
            existente.ClEstatusVisibilidad = "ACTIVO";
            existente.ClOperadorModifica = userEmail;
            existente.NbArtefactoModifica = "VisibilidadService.AsignarVisibilidadAsync";
            existente.FeModificacion = DateTimeOffset.UtcNow;
        }
        else
        {
            // Insert
            existente = new etVisibilidadCatalogo
            {
                IdVisibilidad = Guid.NewGuid(),
                IdCliente = dto.IdCliente,
                IdProducto = idProducto,
                IdVariante = idVariante,
                IdSku = idSku,
                ClTipoAcceso = dto.ClTipoAcceso,
                ClEstatusVisibilidad = "ACTIVO",
                ClOperadorCrea = userEmail,
                NbArtefactoCrea = "VisibilidadService.AsignarVisibilidadAsync",
                FeCreacion = DateTimeOffset.UtcNow
            };
            dbContext.VisibilidadesCatalogo.Add(existente);
        }

        await dbContext.SaveChangesAsync();

        // Recargar con relaciones para el DTO
        var registro = await dbContext.VisibilidadesCatalogo
            .Include(v => v.Cliente)
            .Include(v => v.Producto)
            .Include(v => v.Variante)
                .ThenInclude(var => var!.Combinacion)
            .Include(v => v.Sku)
            .FirstAsync(v => v.IdVisibilidad == existente.IdVisibilidad);

        return new VisibilidadDto(
            registro.IdVisibilidad,
            registro.IdCliente,
            registro.Cliente.NbComercial,
            registro.IdProducto,
            registro.Producto?.NbProducto,
            registro.IdVariante,
            registro.Variante?.Combinacion?.NbCatalogoElemento,
            registro.IdSku,
            registro.Sku?.ClItem,
            registro.ClTipoAcceso,
            registro.ClEstatusVisibilidad
        );
    }

    public async Task<bool> RevocarVisibilidadAsync(Guid idCliente, Guid idProducto)
    {
        var registro = await dbContext.VisibilidadesCatalogo
            .FirstOrDefaultAsync(v => v.IdCliente == idCliente && v.IdProducto == idProducto && v.IdVariante == null && v.IdSku == null);

        if (registro == null) return false;

        dbContext.VisibilidadesCatalogo.Remove(registro);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevocarVisibilidadPorIdAsync(Guid idVisibilidad)
    {
        var registro = await dbContext.VisibilidadesCatalogo
            .FirstOrDefaultAsync(v => v.IdVisibilidad == idVisibilidad);

        if (registro == null) return false;

        dbContext.VisibilidadesCatalogo.Remove(registro);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<VisibilidadDto>> ObtenerClientesDeProductoAsync(Guid idProducto)
    {
        return await dbContext.VisibilidadesCatalogo
            .Include(v => v.Cliente)
            .Include(v => v.Producto)
            .Include(v => v.Variante)
                .ThenInclude(var => var!.Combinacion)
            .Include(v => v.Sku)
            .Where(v => v.IdProducto == idProducto && v.ClEstatusVisibilidad == "ACTIVO")
            .AsNoTracking()
            .Select(v => new VisibilidadDto(
                v.IdVisibilidad,
                v.IdCliente,
                v.Cliente.NbComercial,
                v.IdProducto,
                v.Producto != null ? v.Producto.NbProducto : null,
                v.IdVariante,
                v.Variante != null && v.Variante.Combinacion != null ? v.Variante.Combinacion.NbCatalogoElemento : null,
                v.IdSku,
                v.Sku != null ? v.Sku.ClItem : null,
                v.ClTipoAcceso,
                v.ClEstatusVisibilidad
            ))
            .ToListAsync();
    }

    public async Task<VisibilidadBulkResponse> AsignarVisibilidadBulkAsync(VisibilidadBulkRequest request, string userEmail)
    {
        var ahora = DateTimeOffset.UtcNow;
        var idsProductos = request.IdsProductos.Distinct().ToList();

        // 1. Cargar registros existentes para este cliente + estos productos (un solo query)
        var existentes = await dbContext.VisibilidadesCatalogo
            .Where(v => v.IdCliente == request.IdCliente && v.IdProducto.HasValue && idsProductos.Contains(v.IdProducto.Value))
            .ToListAsync();

        var existentesDict = existentes.Where(v => v.IdProducto.HasValue && !v.IdVariante.HasValue && !v.IdSku.HasValue).ToDictionary(v => v.IdProducto!.Value);

        var insertados = 0;
        var actualizados = 0;

        foreach (var idProducto in idsProductos)
        {
            if (existentesDict.TryGetValue(idProducto, out var registro))
            {
                // Update
                registro.ClTipoAcceso = request.ClTipoAcceso;
                registro.ClEstatusVisibilidad = "ACTIVO";
                registro.ClOperadorModifica = userEmail;
                registro.NbArtefactoModifica = "VisibilidadService.AsignarVisibilidadBulkAsync";
                registro.FeModificacion = ahora;
                actualizados++;
            }
            else
            {
                // Insert
                dbContext.VisibilidadesCatalogo.Add(new etVisibilidadCatalogo
                {
                    IdVisibilidad = Guid.NewGuid(),
                    IdCliente = request.IdCliente,
                    IdProducto = idProducto,
                    ClTipoAcceso = request.ClTipoAcceso,
                    ClEstatusVisibilidad = "ACTIVO",
                    ClOperadorCrea = userEmail,
                    NbArtefactoCrea = "VisibilidadService.AsignarVisibilidadBulkAsync",
                    FeCreacion = ahora
                });
                insertados++;
            }
        }

        // 2. Limpiar reglas hijas redundantes (Variante/SKU) cargadas en existentes
        var hijasRedundantes = existentes.Where(v => (v.IdVariante != null || v.IdSku != null) && v.ClTipoAcceso == request.ClTipoAcceso).ToList();
        if (hijasRedundantes.Any())
        {
            dbContext.VisibilidadesCatalogo.RemoveRange(hijasRedundantes);
        }

        // 3. Un solo SaveChanges — una sola transacción
        await dbContext.SaveChangesAsync();

        return new VisibilidadBulkResponse(
            TotalRecibidos: idsProductos.Count,
            Insertados: insertados,
            Actualizados: actualizados,
            Errores: new List<string>()
        );
    }

    public async Task<bool> TieneAccesoASkuAsync(Guid idCliente, Guid idProducto, Guid idVariante, Guid idSku)
    {
        // 1. Obtener reglas explícitas para ESTE cliente relacionadas con este SKU/Variante/Producto
        var reglasCliente = await dbContext.VisibilidadesCatalogo
            .Where(v => v.IdCliente == idCliente && v.ClEstatusVisibilidad == "ACTIVO" &&
                        (v.IdSku == idSku || v.IdVariante == idVariante || v.IdProducto == idProducto))
            .ToListAsync();

        // Nivel SKU
        var reglaSku = reglasCliente.FirstOrDefault(r => r.IdSku == idSku);
        if (reglaSku != null) return reglaSku.ClTipoAcceso == "VISIBLE" || reglaSku.ClTipoAcceso == "EXCLUSIVO";

        // Nivel Variante
        var reglaVariante = reglasCliente.FirstOrDefault(r => r.IdVariante == idVariante);
        if (reglaVariante != null) return reglaVariante.ClTipoAcceso == "VISIBLE" || reglaVariante.ClTipoAcceso == "EXCLUSIVO";

        // Nivel Producto
        var reglaProducto = reglasCliente.FirstOrDefault(r => r.IdProducto == idProducto && r.IdVariante == null && r.IdSku == null);
        if (reglaProducto != null) return reglaProducto.ClTipoAcceso == "VISIBLE" || reglaProducto.ClTipoAcceso == "EXCLUSIVO";

        // Regla 4: Exclusividad Global. ¿Alguien más tiene este SKU, variante o producto como exclusivo?
        var exclusividadesOtros = await dbContext.VisibilidadesCatalogo
            .Where(v => v.IdCliente != idCliente && v.ClEstatusVisibilidad == "ACTIVO" && v.ClTipoAcceso == "EXCLUSIVO" &&
                        (v.IdSku == idSku || v.IdVariante == idVariante || v.IdProducto == idProducto))
            .ToListAsync();

        if (exclusividadesOtros.Any(r => r.IdSku == idSku)) return false;
        if (exclusividadesOtros.Any(r => r.IdVariante == idVariante)) return false;
        if (exclusividadesOtros.Any(r => r.IdProducto == idProducto && r.IdVariante == null && r.IdSku == null)) return false;

        // Por defecto: Whitelist estricta, si no está explícitamente visible, está oculto.
        return false;
    }

    public async Task<Dictionary<Guid, bool>> EvaluarAccesoSkusMasivoAsync(
        Guid idCliente, IEnumerable<Guid> productoIds, IEnumerable<Guid> varianteIds, IEnumerable<Guid> skuIds)
    {
        var pIds = productoIds.Distinct().ToList();
        var vIds = varianteIds.Distinct().ToList();
        var sIds = skuIds.Distinct().ToList();

        // Cargar todas las reglas del cliente para estos productos, variantes y SKUs
        var reglasCliente = await dbContext.VisibilidadesCatalogo
            .Where(v => v.IdCliente == idCliente && v.ClEstatusVisibilidad == "ACTIVO" &&
                        (sIds.Contains(v.IdSku ?? Guid.Empty) || vIds.Contains(v.IdVariante ?? Guid.Empty) || pIds.Contains(v.IdProducto ?? Guid.Empty)))
            .ToListAsync();

        // Cargar todas las reglas exclusivas de otros clientes
        var exclusividadesOtros = await dbContext.VisibilidadesCatalogo
            .Where(v => v.IdCliente != idCliente && v.ClEstatusVisibilidad == "ACTIVO" && v.ClTipoAcceso == "EXCLUSIVO" &&
                        (sIds.Contains(v.IdSku ?? Guid.Empty) || vIds.Contains(v.IdVariante ?? Guid.Empty) || pIds.Contains(v.IdProducto ?? Guid.Empty)))
            .ToListAsync();

        var resultado = new Dictionary<Guid, bool>();

        // Para evitar evaluar la misma combinación, agruparemos las reglas
        var reglasClienteSku = reglasCliente.Where(r => r.IdSku.HasValue).ToDictionary(r => r.IdSku!.Value);
        var reglasClienteVariante = reglasCliente.Where(r => r.IdVariante.HasValue).ToDictionary(r => r.IdVariante!.Value);
        var reglasClienteProducto = reglasCliente.Where(r => r.IdProducto.HasValue && !r.IdVariante.HasValue && !r.IdSku.HasValue).ToDictionary(r => r.IdProducto!.Value);

        var exclusionesSku = exclusividadesOtros.Where(r => r.IdSku.HasValue).Select(r => r.IdSku!.Value).ToHashSet();
        var exclusionesVariante = exclusividadesOtros.Where(r => r.IdVariante.HasValue).Select(r => r.IdVariante!.Value).ToHashSet();
        var exclusionesProducto = exclusividadesOtros.Where(r => r.IdProducto.HasValue && !r.IdVariante.HasValue && !r.IdSku.HasValue).Select(r => r.IdProducto!.Value).ToHashSet();

        // Este diccionario no es el resultado final porque un SKU pertenece a una Variante que pertenece a un Producto
        // El llamador de este método no provee la jerarquía de un SKU (a qué variante/producto pertenece).
        // Wait, para evaluar eficientemente, necesitamos la relación SKU -> Variante -> Producto.
        // Como no la tenemos aquí, necesitamos obtenerla de la BD.
        var jerarquiaSkus = await dbContext.Skus
            .Where(s => sIds.Contains(s.IdSku))
            .Select(s => new { s.IdSku, s.IdVariante, s.Variante.IdProducto })
            .ToListAsync();

        foreach (var sku in jerarquiaSkus)
        {
            var idSku = sku.IdSku;
            var idVariante = sku.IdVariante;
            var idProducto = sku.IdProducto;

            bool tieneAcceso = false;

            if (reglasClienteSku.TryGetValue(idSku, out var reglaSku))
            {
                tieneAcceso = reglaSku.ClTipoAcceso == "VISIBLE" || reglaSku.ClTipoAcceso == "EXCLUSIVO";
            }
            else if (reglasClienteVariante.TryGetValue(idVariante, out var reglaVariante))
            {
                tieneAcceso = reglaVariante.ClTipoAcceso == "VISIBLE" || reglaVariante.ClTipoAcceso == "EXCLUSIVO";
            }
            else if (reglasClienteProducto.TryGetValue(idProducto, out var reglaProducto))
            {
                tieneAcceso = reglaProducto.ClTipoAcceso == "VISIBLE" || reglaProducto.ClTipoAcceso == "EXCLUSIVO";
            }
            else
            {
                if (exclusionesSku.Contains(idSku)) tieneAcceso = false;
                else if (exclusionesVariante.Contains(idVariante)) tieneAcceso = false;
                else if (exclusionesProducto.Contains(idProducto)) tieneAcceso = false;
                else tieneAcceso = false; // Whitelist estricta
            }

            resultado[idSku] = tieneAcceso;
        }

        return resultado;
    }
}
