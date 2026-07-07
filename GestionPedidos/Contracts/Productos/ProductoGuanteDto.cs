namespace GestionPedidos.Contracts.Productos;

public record ProductoGuanteCreateDto(
    string? ClProducto,
    string NbProducto,
    int IdElemCategoria,
    int? IdElemLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    string? NbPalma,
    string? DsComposicion,
    string? ClMsCode,
    string? ClIndicePalma,
    string? DsForro,
    string? DsCierre,
    string? DsHomologacion,
    List<VarianteNestedCreateDto>? Variantes
);

public record VarianteNestedCreateDto(
    int? IdElemCombinacion,
    string? UrlImagen,
    string ClEstatusVariante,
    List<SkuNestedCreateDto>? Skus
);

public record SkuNestedCreateDto(
    int? IdElemTalla,
    string? ClItem,
    string? ClCodigoBarras,
    string ClEstatusSku,
    int NoStockDisponible,
    int NoStockReservado
);

public record ProductoGuanteUpdateDto(
    string? ClProducto,
    string NbProducto,
    int IdElemCategoria,
    int? IdElemLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    string? NbPalma,
    string? DsComposicion,
    string? ClMsCode,
    string? ClIndicePalma,
    string? DsForro,
    string? DsCierre,
    string? DsHomologacion,
    List<VarianteNestedUpdateDto>? Variantes
);

public record VarianteNestedUpdateDto(
    Guid? IdVariante,
    int? IdElemCombinacion,
    string? UrlImagen,
    string ClEstatusVariante,
    List<SkuNestedUpdateDto>? Skus
);

public record SkuNestedUpdateDto(
    Guid? IdSku,
    int? IdElemTalla,
    string? ClItem,
    string? ClCodigoBarras,
    string ClEstatusSku,
    int NoStockDisponible,
    int NoStockReservado
);

public record ProductoGuanteDto(
    Guid IdProducto,
    string? ClProducto,
    string NbProducto,
    int IdElemCategoria,
    int? IdElemLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    string? NbPalma,
    string? DsComposicion,
    string? ClMsCode,
    string? ClIndicePalma,
    string? DsForro,
    string? DsCierre,
    string? DsHomologacion,
    DateTimeOffset FeCreacion,
    DateTimeOffset? FeModificacion,
    List<GestionPedidos.Contracts.Variantes.VarianteDto>? Variantes = null
);

public record GuanteCatalogoDto(
    Guid IdProducto,
    Guid IdVariante,
    string ClProducto,
    string NbProducto,
    string? UrlImagen,
    string? DsColor,
    string Tallas,
    decimal PrecioBase,
    string? DsCategoria,
    string Estatus
);
