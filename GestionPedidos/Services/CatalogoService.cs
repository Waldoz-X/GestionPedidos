using GestionPedidos.Contracts.Catalogo;
using GestionPedidos.Data;
using GestionPedidos.Models.Catalogo;
using Microsoft.EntityFrameworkCore;

namespace GestionPedidos.Services;

/// <summary>
/// Servicio dinámico que opera sobre CUALQUIER catálogo del sistema.
/// En lugar de un servicio por cada catálogo (MarcasService, DivisionesService, etc.),
/// un solo servicio resuelve todo recibiendo la clave del catálogo como parámetro.
/// </summary>
public sealed class CatalogoService : ICatalogoService
{
    private readonly AppDbContext _db;

    public CatalogoService(AppDbContext db) => _db = db;

    // ════════════════════════════════════════════════════════════════
    //  CATÁLOGOS MAESTROS
    // ════════════════════════════════════════════════════════════════

    public async Task<IReadOnlyList<CatalogoDto>> GetCatalogosAsync()
    {
        return await _db.CCatalogos
            .AsNoTracking()
            .Where(c => c.ClEstatusCatalogo == "ACTIVO")
            .Select(c => new CatalogoDto
            {
                IdCatalogo       = c.IdCatalogo,
                ClCatalogo       = c.ClCatalogo,
                NbCatalogo       = c.NbCatalogo,
                DsCatalogo       = c.DsCatalogo,
                IdCatalogoPadre  = c.IdCatalogoPadre,
                ClEstatusCatalogo = c.ClEstatusCatalogo,
                TotalElementos   = c.Elementos.Count(e => e.ClEstatusCatalogoElemento == "ACTIVO")
            })
            .OrderBy(c => c.NbCatalogo)
            .ToListAsync();
    }

    public async Task<CatalogoDto> CrearCatalogoAsync(CrearCatalogoRequest request, string operador)
    {
        var existe = await _db.CCatalogos.AnyAsync(c => c.ClCatalogo == request.ClCatalogo.ToUpperInvariant());
        if (existe)
        {
            throw new ArgumentException($"El catálogo con clave '{request.ClCatalogo.ToUpperInvariant()}' ya existe.");
        }

        var entity = new CCatalogo
        {
            ClCatalogo = request.ClCatalogo.ToUpperInvariant(),
            NbCatalogo = request.NbCatalogo,
            DsCatalogo = request.DsCatalogo,
            IdCatalogoPadre = request.IdCatalogoPadre,
            ClEstatusCatalogo = "ACTIVO",
            ClOperadorCrea = operador,
            NbArtefactoCrea = "API.Catalogos",
            FeCreacion = DateTimeOffset.UtcNow
        };

        _db.CCatalogos.Add(entity);
        await _db.SaveChangesAsync();

        return new CatalogoDto
        {
            IdCatalogo = entity.IdCatalogo,
            ClCatalogo = entity.ClCatalogo,
            NbCatalogo = entity.NbCatalogo,
            DsCatalogo = entity.DsCatalogo,
            IdCatalogoPadre = entity.IdCatalogoPadre,
            ClEstatusCatalogo = entity.ClEstatusCatalogo,
            TotalElementos = 0
        };
    }

    public async Task<CatalogoDto?> ActualizarCatalogoAsync(int idCatalogo, ActualizarCatalogoRequest request, string operador)
    {
        var entity = await _db.CCatalogos
            .Include(c => c.Elementos)
            .FirstOrDefaultAsync(c => c.IdCatalogo == idCatalogo);

        if (entity is null) return null;

        if (request.NbCatalogo is not null)
            entity.NbCatalogo = request.NbCatalogo;

        if (request.DsCatalogo is not null)
            entity.DsCatalogo = request.DsCatalogo;

        if (request.IdCatalogoPadre is not null)
            entity.IdCatalogoPadre = request.IdCatalogoPadre;

        if (request.ClEstatusCatalogo is not null)
            entity.ClEstatusCatalogo = request.ClEstatusCatalogo.ToUpperInvariant();

        entity.ClOperadorModifica = operador;
        entity.NbArtefactoModifica = "API.Catalogos";
        entity.FeModificacion = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();

        return new CatalogoDto
        {
            IdCatalogo = entity.IdCatalogo,
            ClCatalogo = entity.ClCatalogo,
            NbCatalogo = entity.NbCatalogo,
            DsCatalogo = entity.DsCatalogo,
            IdCatalogoPadre = entity.IdCatalogoPadre,
            ClEstatusCatalogo = entity.ClEstatusCatalogo,
            TotalElementos = entity.Elementos.Count(e => e.ClEstatusCatalogoElemento == "ACTIVO")
        };
    }

    public async Task<bool> EliminarCatalogoAsync(int idCatalogo, string operador)
    {
        var entity = await _db.CCatalogos
            .Include(c => c.Elementos)
            .FirstOrDefaultAsync(c => c.IdCatalogo == idCatalogo);

        if (entity is null) return false;

        entity.ClEstatusCatalogo = "INACTIVO";
        entity.ClOperadorModifica = operador;
        entity.NbArtefactoModifica = "API.Catalogos";
        entity.FeModificacion = DateTimeOffset.UtcNow;

        // Borrado lógico en cascada para todos los elementos del catálogo
        foreach (var elemento in entity.Elementos)
        {
            elemento.ClEstatusCatalogoElemento = "INACTIVO";
            elemento.ClOperadorModifica = operador;
            elemento.NbArtefactoModifica = "API.Catalogos";
            elemento.FeModificacion = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync();
        return true;
    }

    // ════════════════════════════════════════════════════════════════
    //  ELEMENTOS – CRUD
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Endpoint universal: devuelve los elementos activos de un catálogo dado su código.
    /// Ejemplo: clCatalogo = "GAMAS" → devuelve PRO, PRIME, AERO, etc.
    /// </summary>
    public async Task<IReadOnlyList<CatalogoElementoDto>> GetElementosAsync(string clCatalogo)
    {
        return await _db.CCatalogoElementos
            .AsNoTracking()
            .Include(e => e.Catalogo)
            .Include(e => e.ElementoPadre)
            .Where(e => e.Catalogo.ClCatalogo == clCatalogo
                     && e.ClEstatusCatalogoElemento == "ACTIVO")
            .OrderBy(e => e.NbCatalogoElemento)
            .Select(e => MapToDto(e))
            .ToListAsync();
    }

    public async Task<CatalogoElementoDto?> GetElementoByIdAsync(int idElemento)
    {
        var entity = await _db.CCatalogoElementos
            .AsNoTracking()
            .Include(e => e.Catalogo)
            .Include(e => e.ElementoPadre)
            .FirstOrDefaultAsync(e => e.IdCatalogoElemento == idElemento);

        return entity is null ? null : MapToDto(entity);
    }

    public async Task<CatalogoElementoDto> CrearElementoAsync(
        string clCatalogo,
        CrearCatalogoElementoRequest request,
        string operador)
    {
        var catalogo = await _db.CCatalogos
            .FirstOrDefaultAsync(c => c.ClCatalogo == clCatalogo)
            ?? throw new KeyNotFoundException($"No existe el catálogo '{clCatalogo}'.");

        var entity = new CCatalogoElemento
        {
            IdCatalogo                = catalogo.IdCatalogo,
            ClCatalogoElemento        = request.ClCatalogoElemento.ToUpperInvariant(),
            NbCatalogoElemento        = request.NbCatalogoElemento,
            DsCatalogoElemento        = request.DsCatalogoElemento,
            IdCatalogoElementoPadre   = request.IdCatalogoElementoPadre,
            ClEstatusCatalogoElemento = "ACTIVO",
            ClOperadorCrea            = operador,
            NbArtefactoCrea           = "API.Catalogos"
        };

        _db.CCatalogoElementos.Add(entity);
        await _db.SaveChangesAsync();

        return MapToDto(entity);
    }

    public async Task<CatalogoElementoDto?> ActualizarElementoAsync(
        int idElemento,
        ActualizarCatalogoElementoRequest request,
        string operador)
    {
        var entity = await _db.CCatalogoElementos
            .FirstOrDefaultAsync(e => e.IdCatalogoElemento == idElemento);

        if (entity is null) return null;

        // Solo sobreescribir los campos que llegan con valor
        if (request.NbCatalogoElemento is not null)
            entity.NbCatalogoElemento = request.NbCatalogoElemento;

        if (request.DsCatalogoElemento is not null)
            entity.DsCatalogoElemento = request.DsCatalogoElemento;

        if (request.IdCatalogoElementoPadre is not null)
            entity.IdCatalogoElementoPadre = request.IdCatalogoElementoPadre;

        if (request.ClEstatusCatalogoElemento is not null)
            entity.ClEstatusCatalogoElemento = request.ClEstatusCatalogoElemento.ToUpperInvariant();

        entity.ClOperadorModifica  = operador;
        entity.NbArtefactoModifica = "API.Catalogos";
        entity.FeModificacion      = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();
        return MapToDto(entity);
    }

    /// <summary>
    /// Eliminación lógica (soft-delete): cambia el estatus a INACTIVO.
    /// Nunca borramos catálogos físicamente porque tienen FKs por todo el sistema.
    /// </summary>
    public async Task<bool> EliminarElementoAsync(int idElemento, string operador)
    {
        var entity = await _db.CCatalogoElementos
            .Include(e => e.ElementosHijos)
            .FirstOrDefaultAsync(e => e.IdCatalogoElemento == idElemento);

        if (entity is null) return false;

        entity.ClEstatusCatalogoElemento = "INACTIVO";
        entity.ClOperadorModifica  = operador;
        entity.NbArtefactoModifica = "API.Catalogos";
        entity.FeModificacion      = DateTimeOffset.UtcNow;

        // Borrado lógico en cascada para los elementos hijos (1 nivel)
        foreach (var hijo in entity.ElementosHijos)
        {
            hijo.ClEstatusCatalogoElemento = "INACTIVO";
            hijo.ClOperadorModifica = operador;
            hijo.NbArtefactoModifica = "API.Catalogos";
            hijo.FeModificacion = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync();
        return true;
    }

    // ════════════════════════════════════════════════════════════════
    //  MAPPING
    // ════════════════════════════════════════════════════════════════

    private static CatalogoElementoDto MapToDto(CCatalogoElemento e) => new()
    {
        IdCatalogoElemento      = e.IdCatalogoElemento,
        ClCatalogoElemento      = e.ClCatalogoElemento,
        NbCatalogoElemento      = e.NbCatalogoElemento,
        DsCatalogoElemento      = e.DsCatalogoElemento,
        IdCatalogoElementoPadre = e.IdCatalogoElementoPadre,
        NbCatalogo              = e.Catalogo?.NbCatalogo,
        NbCatalogoElementoPadre = e.ElementoPadre?.NbCatalogoElemento,
        ClEstatusCatalogoElemento = e.ClEstatusCatalogoElemento
    };
}
