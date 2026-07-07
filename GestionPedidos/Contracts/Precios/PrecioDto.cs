namespace GestionPedidos.Contracts.Precios;

/// <summary>
/// Resultado del Motor de Precios en Cascada.
/// Angular solo lee este DTO: sabe el precio final y la moneda. Nada más.
/// </summary>
public record PrecioResueltoDto(
    Guid IdSku,
    bool PrecioEncontrado,
    decimal? PrecioFinal,
    string? Moneda,
    int NivelAplicado,           // 1 = Precio directo del cliente | 2 = Precio por política
    string? NbPoliticaAplicada,  // Nombre de la política si fue nivel 2
    DateTimeOffset? VigenteHasta,
    string? Mensaje              // Mensaje si no se encontró precio
);

/// <summary>
/// Para crear o actualizar un precio individual desde el panel de Admin.
/// </summary>
public record PrecioUpsertDto(
    Guid IdSku,
    Guid? IdCliente,     // null = precio de política (aplica a todos los clientes de esa política)
    Guid? IdPolitica,    // null = precio exclusivo directo del cliente
    decimal MnPrecioNeto,
    string ClMoneda,     // "EUR", "MXN", "USD", etc.
    DateTimeOffset FeVigenteDesde,
    DateTimeOffset? FeVigenteHasta
);

/// <summary>
/// Para carga masiva de precios desde Python/Excel.
/// Usa claves de texto para no depender de IDs.
/// </summary>
public record PrecioBulkDto(
    string ClItemOCodigoBarras,   // Clave del SKU tal como sale del Excel
    string? NbComercialCliente,   // Nombre exacto del cliente (null = precio de política)
    string? NbPolitica,           // Nombre exacto de la política (null = precio directo)
    decimal MnPrecioNeto,
    string ClMoneda,
    string? FeVigenteHasta        // Texto "2026-12-31" o null
);

/// <summary>
/// Respuesta de la carga masiva con resumen de lo que se procesó.
/// </summary>
public record PrecioBulkResultDto(
    int TotalRecibidos,
    int Insertados,
    int Actualizados,
    List<string> Errores
);

/// <summary>
/// Para ver el historial de cambios de un precio.
/// </summary>
public record HistorialPrecioDto(
    Guid Id,
    decimal PrecioAnterior,
    decimal PrecioNuevo,
    string NbUsuario,
    DateTimeOffset RegistradoEn
);

/// <summary>
/// Vista de un precio en el listado del Admin.
/// </summary>
public record PrecioDto(
    Guid IdPrecio,
    Guid IdSku,
    string? ClItem,
    Guid? IdCliente,
    string? NbComercialCliente,
    Guid? IdPolitica,
    string? NbPolitica,
    decimal MnPrecioNeto,
    string ClMoneda,
    string ClEstatusPrecio,
    DateTimeOffset FeVigenteDesde,
    DateTimeOffset? FeVigenteHasta
);
