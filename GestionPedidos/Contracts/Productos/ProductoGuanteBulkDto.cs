namespace GestionPedidos.Contracts.Productos;

/// <summary>
/// DTO para carga masiva de guantes.
/// En lugar de IDs numéricos, recibe las claves de texto de los catálogos
/// tal cual salen del Excel. El backend se encarga de traducirlos a IDs reales.
/// </summary>
public record ProductoGuanteBulkDto(
    string? ClProducto,           // Clave del producto (ej: "ARPRAD")
    string NbProducto,            // Nombre del producto (ej: "ARKANO PRO")
    string ClCategoria,           // Clave de la categoría/gama (ej: "MASTER", "PROFESIONAL", "PP")
    string? ClLineaColeccion,     // Clave de línea/colección (ej: "GUA26")
    string? ClHsCode,
    string ClEstatusProducto,
    string? NbPalma,
    string? DsComposicion,
    string? ClMsCode,
    string? ClIndicePalma,
    string? DsForro,
    string? DsCierre,
    string? DsHomologacion,
    List<VarianteBulkDto>? Variantes
);

public record VarianteBulkDto(
    string? ClCombinacion,        // Clave de la combinación de color (ej: "NEGRO/BLANCO/ROJO")
    string? UrlImagen,
    string ClEstatusVariante,
    List<SkuBulkDto>? Skus
);

public record SkuBulkDto(
    string? ClTalla,              // Clave de la talla (ej: "3", "4", "5", "7")
    string? ClItem,
    string? ClCodigoBarras,
    string ClEstatusSku,
    int NoStockDisponible,
    int NoStockReservado
);
