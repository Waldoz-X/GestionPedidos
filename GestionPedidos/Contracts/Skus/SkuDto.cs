namespace GestionPedidos.Contracts.Skus;

public record SkuCreateDto(
    Guid IdVariante,
    int? IdElemTalla,
    string? ClItem,
    string? ClCodigoBarras,
    string ClEstatusSku,
    int NoStockDisponible,
    int NoStockReservado
);

public record SkuUpdateDto(
    int? IdElemTalla,
    string? ClItem,
    string? ClCodigoBarras,
    string ClEstatusSku,
    int NoStockDisponible,
    int NoStockReservado
);

public record SkuDto(
    Guid IdSku,
    Guid IdVariante,
    int? IdElemTalla,
    string? ClItem,
    string? ClCodigoBarras,
    string ClEstatusSku,
    int NoStockDisponible,
    int NoStockReservado,
    DateTimeOffset FeCreacion,
    DateTimeOffset? FeModificacion
);
