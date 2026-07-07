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

    /// <summary>Lista todos los clientes que tienen acceso a un producto dado.</summary>
    Task<IEnumerable<VisibilidadDto>> ObtenerClientesDeProductoAsync(Guid idProducto);
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
                .ThenInclude(p => p.Categoria)
            .Include(v => v.Producto)
                .ThenInclude(p => p.LineaColeccion)
            .Where(v =>
                v.IdCliente == idCliente &&
                v.ClEstatusVisibilidad == "ACTIVO" &&
                (v.ClTipoAcceso == "VISIBLE" || v.ClTipoAcceso == "EXCLUSIVO") &&
                v.Producto.ClEstatusProducto == "ACTIVO")
            .AsNoTracking()
            .Select(v => new ProductoVisibleDto(
                v.Producto.IdProducto,
                v.Producto.ClProducto,
                v.Producto.NbProducto,
                v.Producto.Categoria.NbCatalogoElemento,
                v.Producto.LineaColeccion != null ? v.Producto.LineaColeccion.NbCatalogoElemento : null,
                v.ClTipoAcceso
            ))
            .ToListAsync();
    }

    public async Task<VisibilidadDto> AsignarVisibilidadAsync(VisibilidadUpsertDto dto, string userEmail)
    {
        var existente = await dbContext.VisibilidadesCatalogo
            .FirstOrDefaultAsync(v => v.IdCliente == dto.IdCliente && v.IdProducto == dto.IdProducto);

        if (existente != null)
        {
            // Update
            existente.ClTipoAcceso = dto.ClTipoAcceso;
            existente.ClOperadorModifica = userEmail;
            existente.NbArtefactoModifica = "VisibilidadService.AsignarVisibilidadAsync";
            existente.FeModificacion = DateTimeOffset.UtcNow;
        }
        else
        {
            // Insert
            existente = new etVisibilidadCatalogo
            {
                IdCliente = dto.IdCliente,
                IdProducto = dto.IdProducto,
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
            .FirstAsync(v => v.IdCliente == dto.IdCliente && v.IdProducto == dto.IdProducto);

        return new VisibilidadDto(
            registro.IdCliente,
            registro.Cliente.NbComercial,
            registro.IdProducto,
            registro.Producto.NbProducto,
            registro.ClTipoAcceso,
            registro.ClEstatusVisibilidad
        );
    }

    public async Task<bool> RevocarVisibilidadAsync(Guid idCliente, Guid idProducto)
    {
        var registro = await dbContext.VisibilidadesCatalogo
            .FirstOrDefaultAsync(v => v.IdCliente == idCliente && v.IdProducto == idProducto);

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
            .Where(v => v.IdProducto == idProducto && v.ClEstatusVisibilidad == "ACTIVO")
            .AsNoTracking()
            .Select(v => new VisibilidadDto(
                v.IdCliente,
                v.Cliente.NbComercial,
                v.IdProducto,
                v.Producto.NbProducto,
                v.ClTipoAcceso,
                v.ClEstatusVisibilidad
            ))
            .ToListAsync();
    }
}
