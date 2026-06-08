namespace GestionPedidos.Contracts.Clientes;

public record ClienteCreateDto(
    string NbComercial,
    string ClTipoCliente,
    int IdElemMoneda,
    string? DsCanalVenta,
    decimal MnLimiteCredito,
    string ClEstatusCliente
);

public record ClienteUpdateDto(
    string NbComercial,
    string ClTipoCliente,
    int IdElemMoneda,
    string? DsCanalVenta,
    decimal MnLimiteCredito,
    string ClEstatusCliente
);

public record ClienteDto(
    Guid IdCliente,
    string NbComercial,
    string ClTipoCliente,
    int IdElemMoneda,
    string? ClMoneda,
    string? NbMoneda,
    string? DsCanalVenta,
    decimal MnLimiteCredito,
    string ClEstatusCliente,
    DateTimeOffset FeCreacion,
    DateTimeOffset? FeModificacion
);
