namespace GestionPedidos.Contracts.Variantes;

public record VarianteCreateDto(
    Guid IdProducto,
    int? IdElemCombinacion,
    string? UrlImagen,
    string ClEstatusVariante
);

public record VarianteUpdateDto(
    int? IdElemCombinacion,
    string? UrlImagen,
    string ClEstatusVariante
);

public record VarianteDto(
    Guid IdVariante,
    Guid IdProducto,
    int? IdElemCombinacion,
    string? UrlImagen,
    string ClEstatusVariante,
    DateTimeOffset FeCreacion,
    DateTimeOffset? FeModificacion,
    List<GestionPedidos.Contracts.Skus.SkuDto>? Skus = null
);
