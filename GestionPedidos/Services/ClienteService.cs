using GestionPedidos.Contracts.Clientes;
using GestionPedidos.Data;
using GestionPedidos.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPedidos.Services;

public interface IClienteService
{
    Task<IEnumerable<ClienteDto>> ObtenerTodosAsync();
    Task<ClienteDto?> ObtenerPorIdAsync(Guid idCliente);
    Task<ClienteDto> CrearAsync(ClienteCreateDto dto, string userEmail);
    Task<ClienteDto?> ActualizarAsync(Guid idCliente, ClienteUpdateDto dto, string userEmail);
    Task<bool> EliminarAsync(Guid idCliente);
}

public class ClienteService(AppDbContext dbContext) : IClienteService
{
    public async Task<IEnumerable<ClienteDto>> ObtenerTodosAsync()
    {
        var clientes = await dbContext.Clientes
            .Include(c => c.ClMoneda)
            .AsNoTracking()
            .ToListAsync();

        return clientes.Select(MapToDto);
    }

    public async Task<ClienteDto?> ObtenerPorIdAsync(Guid idCliente)
    {
        var cliente = await dbContext.Clientes
            .Include(c => c.ClMoneda)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdCliente == idCliente);

        return cliente == null ? null : MapToDto(cliente);
    }

    public async Task<ClienteDto> CrearAsync(ClienteCreateDto dto, string userEmail)
    {
        var cliente = new etCliente
        {
            IdCliente = Guid.NewGuid(),
            NbComercial = dto.NbComercial,
            ClTipoCliente = dto.ClTipoCliente,
            IdElemMoneda = dto.IdElemMoneda,
            DsCanalVenta = dto.DsCanalVenta,
            MnLimiteCredito = dto.MnLimiteCredito,
            ClEstatusCliente = dto.ClEstatusCliente,
            ClOperadorCrea = userEmail,
            NbArtefactoCrea = "ClienteService.CrearAsync"
        };

        dbContext.Clientes.Add(cliente);
        await dbContext.SaveChangesAsync();
        
        await dbContext.Entry(cliente).Reference(c => c.ClMoneda).LoadAsync();

        return MapToDto(cliente);
    }

    public async Task<ClienteDto?> ActualizarAsync(Guid idCliente, ClienteUpdateDto dto, string userEmail)
    {
        var cliente = await dbContext.Clientes
            .FirstOrDefaultAsync(c => c.IdCliente == idCliente);

        if (cliente == null) return null;

        cliente.NbComercial = dto.NbComercial;
        cliente.ClTipoCliente = dto.ClTipoCliente;
        cliente.IdElemMoneda = dto.IdElemMoneda;
        cliente.DsCanalVenta = dto.DsCanalVenta;
        cliente.MnLimiteCredito = dto.MnLimiteCredito;
        cliente.ClEstatusCliente = dto.ClEstatusCliente;

        cliente.ClOperadorModifica = userEmail;
        cliente.NbArtefactoModifica = "ClienteService.ActualizarAsync";
        cliente.FeModificacion = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync();
        await dbContext.Entry(cliente).Reference(c => c.ClMoneda).LoadAsync();
        return MapToDto(cliente);
    }

    public async Task<bool> EliminarAsync(Guid idCliente)
    {
        var cliente = await dbContext.Clientes
            .FirstOrDefaultAsync(c => c.IdCliente == idCliente);

        if (cliente == null) return false;

        dbContext.Clientes.Remove(cliente);
        await dbContext.SaveChangesAsync();
        return true;
    }

    private static ClienteDto MapToDto(etCliente c)
    {
        return new ClienteDto(
            c.IdCliente,
            c.NbComercial,
            c.ClTipoCliente,
            c.IdElemMoneda,
            c.ClMoneda?.ClCatalogoElemento,
            c.ClMoneda?.NbCatalogoElemento,
            c.DsCanalVenta,
            c.MnLimiteCredito,
            c.ClEstatusCliente,
            c.FeCreacion,
            c.FeModificacion
        );
    }
}
