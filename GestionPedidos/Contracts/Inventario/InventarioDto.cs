using System;

namespace GestionPedidos.Contracts.Inventario;

public record RegistrarMovimientoRequest(
    Guid IdSku,
    int NoCantidad,
    string? DsMotivo
);

public record RegistrarAjusteRequest(
    Guid IdSku,
    int NoStockFisicoReal, // La cantidad física contada (ej. 50)
    string? DsMotivo
);

public record MovimientoInventarioDto(
    Guid IdMovimiento,
    Guid IdSku,
    int NoCantidad,
    string ClTipoMovimiento,
    string? DsMotivo,
    string ClOperadorCrea,
    DateTimeOffset FeCreacion
);

public record LibroAuditoriaDto(
    Guid IdMovimiento,
    DateTimeOffset FeCreacion,
    string ClTipoMovimiento,
    Guid IdSku,
    string? ClItem,
    string? NbProducto,
    string? NbCombinacion, // Color
    string? NbTalla,
    int NoCantidad,
    string ClOperadorCrea,
    string? DsMotivo
);

public record StockRealDto(
    Guid IdSku,
    string? ClItem,
    string? NbProducto,
    string? NbCombinacion, // Color
    string? NbTalla,
    int NoStockDisponible,
    int NoStockReservado,
    int NoStockNeto,
    int NoStockMinimo,
    decimal MnPrecioBase, // Precio general de lista
    string ClSemaforoStock // "SIN_STOCK" | "STOCK_BAJO" | "STOCK_OK"
);

public record RotacionSkuDto(
    Guid IdSku,
    string? ClItem,
    string? NbProducto,
    string? NbCombinacion, // Color
    string? NbTalla,
    int NoCantidadVendida
);
