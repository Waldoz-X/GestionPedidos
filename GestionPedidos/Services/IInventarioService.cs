using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GestionPedidos.Contracts.Inventario;

namespace GestionPedidos.Services;

public interface IInventarioService
{
    Task<MovimientoInventarioDto> RegistrarEntradaAsync(RegistrarMovimientoRequest request, string userEmail);
    Task<MovimientoInventarioDto> RegistrarBajaAsync(RegistrarMovimientoRequest request, string userEmail);
    Task<MovimientoInventarioDto> RegistrarAjusteAsync(RegistrarAjusteRequest request, string userEmail);
    Task<IEnumerable<MovimientoInventarioDto>> ObtenerKardexSkuAsync(Guid idSku);
    Task<IEnumerable<LibroAuditoriaDto>> ObtenerLibroAuditoriaAsync(string? clTipoMovimiento, DateTimeOffset? feInicio, DateTimeOffset? feFin);
    Task<IEnumerable<StockRealDto>> ObtenerStockRealAsync();
    Task<IEnumerable<RotacionSkuDto>> ObtenerRotacionInventarioAsync(DateTimeOffset feInicio, DateTimeOffset feFin);
}
