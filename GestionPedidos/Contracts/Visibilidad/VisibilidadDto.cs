namespace GestionPedidos.Contracts.Visibilidad;

/// <summary>
/// Información de visibilidad de un producto para un cliente específico.
/// </summary>
public record VisibilidadDto(
    Guid IdCliente,
    string NbComercialCliente,
    Guid IdProducto,
    string NbProducto,
    string ClTipoAcceso,         // "VISIBLE" | "EXCLUSIVO" | "OCULTO"
    string ClEstatusVisibilidad
);

/// <summary>
/// Para asignar o actualizar la visibilidad de un producto para un cliente.
/// Solo Admin puede usarlo.
/// </summary>
public record VisibilidadUpsertDto(
    Guid IdCliente,
    Guid IdProducto,
    string ClTipoAcceso          // "VISIBLE" | "EXCLUSIVO" | "OCULTO"
);

/// <summary>
/// Resumen de un producto visible para el cliente loggeado.
/// Incluye solo la info necesaria para pintar la pantalla de catálogo.
/// </summary>
public record ProductoVisibleDto(
    Guid IdProducto,
    string? ClProducto,
    string NbProducto,
    string ClCategoria,
    string? ClLineaColeccion,
    string ClTipoAcceso   // "VISIBLE" | "EXCLUSIVO"
);

/// <summary>
/// Request para asignar visibilidad en bulk.
/// Un solo cliente + N productos + tipo de acceso.
/// </summary>
public record VisibilidadBulkRequest(
    Guid IdCliente,
    List<Guid> IdsProductos,
    string ClTipoAcceso          // "VISIBLE" | "EXCLUSIVO" | "OCULTO"
);

/// <summary>
/// Respuesta del endpoint bulk con contadores de la operación.
/// </summary>
public record VisibilidadBulkResponse(
    int TotalRecibidos,
    int Insertados,
    int Actualizados,
    List<string> Errores
);
