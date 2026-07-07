using GestionPedidos.Contracts.Precios;
using GestionPedidos.Data;
using GestionPedidos.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPedidos.Services;

public interface IPoliticaService
{
    Task<IEnumerable<PoliticaDto>> ObtenerTodasAsync();
    Task<PoliticaDto> CrearAsync(PoliticaUpsertDto dto, string userEmail);
    Task<IEnumerable<ClientePoliticaDto>> ObtenerClientesDePoliticaAsync(Guid idPolitica);
    Task<ClientePoliticaDto> AsignarClienteAsync(ClientePoliticaUpsertDto dto, string userEmail);
    Task<bool> RemoverClienteAsync(Guid idPolitica, Guid idCliente);
}

public class PoliticaService(AppDbContext dbContext) : IPoliticaService
{
    public async Task<IEnumerable<PoliticaDto>> ObtenerTodasAsync()
    {
        return await dbContext.PoliticasPrecios
            .AsNoTracking()
            .Select(p => new PoliticaDto(
                p.IdPolitica,
                p.NbPolitica,
                p.ClTipoPolitica,
                p.NoPrioridad,
                p.MnFactorDescuento,
                p.FeVigenteDesde,
                p.FeVigenteHasta,
                p.ClEstatusPolitica,
                p.Clientes.Count
            ))
            .ToListAsync();
    }

    public async Task<PoliticaDto> CrearAsync(PoliticaUpsertDto dto, string userEmail)
    {
        var politica = new etPoliticaPrecio
        {
            IdPolitica = Guid.NewGuid(),
            NbPolitica = dto.NbPolitica,
            ClTipoPolitica = dto.ClTipoPolitica,
            NoPrioridad = dto.NoPrioridad,
            MnFactorDescuento = dto.MnFactorDescuento,
            FeVigenteDesde = dto.FeVigenteDesde,
            FeVigenteHasta = dto.FeVigenteHasta,
            ClEstatusPolitica = "ACTIVO",
            ClOperadorCrea = userEmail,
            NbArtefactoCrea = "PoliticaService.CrearAsync",
            FeCreacion = DateTimeOffset.UtcNow
        };

        dbContext.PoliticasPrecios.Add(politica);
        await dbContext.SaveChangesAsync();

        return new PoliticaDto(
            politica.IdPolitica,
            politica.NbPolitica,
            politica.ClTipoPolitica,
            politica.NoPrioridad,
            politica.MnFactorDescuento,
            politica.FeVigenteDesde,
            politica.FeVigenteHasta,
            politica.ClEstatusPolitica,
            0
        );
    }

    public async Task<IEnumerable<ClientePoliticaDto>> ObtenerClientesDePoliticaAsync(Guid idPolitica)
    {
        return await dbContext.ClientesPoliticas
            .Include(cp => cp.Cliente)
            .Where(cp => cp.IdPolitica == idPolitica)
            .AsNoTracking()
            .Select(cp => new ClientePoliticaDto(
                cp.IdCliente,
                cp.Cliente.NbComercial,
                cp.IdPolitica,
                cp.EsPrincipal
            ))
            .ToListAsync();
    }

    public async Task<ClientePoliticaDto> AsignarClienteAsync(ClientePoliticaUpsertDto dto, string userEmail)
    {
        var asignacion = await dbContext.ClientesPoliticas
            .FirstOrDefaultAsync(cp => cp.IdCliente == dto.IdCliente && cp.IdPolitica == dto.IdPolitica);

        if (asignacion != null)
        {
            asignacion.EsPrincipal = dto.EsPrincipal;
            asignacion.ClOperadorModifica = userEmail;
            asignacion.NbArtefactoModifica = "PoliticaService.AsignarClienteAsync";
            asignacion.FeModificacion = DateTimeOffset.UtcNow;
        }
        else
        {
            asignacion = new etClientePolitica
            {
                IdCliente = dto.IdCliente,
                IdPolitica = dto.IdPolitica,
                EsPrincipal = dto.EsPrincipal,
                ClOperadorCrea = userEmail,
                NbArtefactoCrea = "PoliticaService.AsignarClienteAsync",
                FeCreacion = DateTimeOffset.UtcNow
            };
            dbContext.ClientesPoliticas.Add(asignacion);
        }

        await dbContext.SaveChangesAsync();

        var cliente = await dbContext.Clientes.FirstAsync(c => c.IdCliente == dto.IdCliente);

        return new ClientePoliticaDto(
            asignacion.IdCliente,
            cliente.NbComercial,
            asignacion.IdPolitica,
            asignacion.EsPrincipal
        );
    }

    public async Task<bool> RemoverClienteAsync(Guid idPolitica, Guid idCliente)
    {
        var asignacion = await dbContext.ClientesPoliticas
            .FirstOrDefaultAsync(cp => cp.IdCliente == idCliente && cp.IdPolitica == idPolitica);

        if (asignacion == null) return false;

        dbContext.ClientesPoliticas.Remove(asignacion);
        await dbContext.SaveChangesAsync();
        return true;
    }
}
