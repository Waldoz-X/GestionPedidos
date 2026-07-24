namespace GestionPedidos.Contracts.Pedidos;

// ════════════════════════════════════════════════════════════════════════════
//  REQUESTS (entrada)
// ════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Crea un pedido en estado BORRADOR.
/// El IdCliente y IdUsuarioCaptura se extraen del JWT — nunca del body.
/// </summary>
public record CrearPedidoRequest(
    Guid? IdDireccionEnvio
);

/// <summary>
/// Agrega una línea al pedido borrador.
/// El precio se resuelve automáticamente por el motor de precios.
/// </summary>
public record AgregarLineaRequest(
    Guid IdSku,
    int NoCantidad
);

/// <summary>
/// Actualiza la cantidad de una línea existente en un borrador.
/// </summary>
public record ActualizarLineaRequest(
    int NoCantidad
);

/// <summary>
/// Solo Admin/Empleado asignado: cambia el estatus del pedido.
/// Transiciones válidas controladas por la máquina de estados del service.
/// </summary>
public record CambiarEstatusRequest(
    string ClEstatusNuevo,   // "FACTURADO" | "ENVIADO" | "CANCELADO"
    string? Notas
);

// ════════════════════════════════════════════════════════════════════════════
//  RESPONSES (salida)
// ════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Vista resumen para listados de pedidos.
/// </summary>
public record PedidoResumenDto(
    Guid IdPedido,
    string ClFolio,
    string ClEstatusPedido,
    string NbCliente,
    string? ClMoneda,
    decimal MnSubtotal,
    decimal MnDescuentoComercial,
    decimal MnTotal,
    int TotalLineas,
    string ResumenProductos,
    DateTimeOffset FePedido,
    DateTimeOffset FeCreacion
);

/// <summary>
/// Vista detalle completa de un pedido con sus líneas, dirección y historial.
/// </summary>
public record PedidoDetalleDto(
    Guid IdPedido,
    string ClFolio,
    string ClEstatusPedido,
    Guid IdCliente,
    string NbCliente,
    string? ClMoneda,
    Guid? IdDireccionEnvio,
    DireccionEnvioDto? DireccionEnvio,
    string? NbPolitica,
    decimal MnSubtotal,
    decimal MnDescuentoComercial,
    decimal MnDescuentoAdmin,
    decimal MnTotal,
    DateTimeOffset FePedido,
    List<LineaPedidoDto> Lineas,
    List<HistorialPedidoDto> Historial
);

/// <summary>
/// Detalle de cada línea del pedido.
/// Incluye info del SKU para que el frontend pueda renderizar sin queries extra.
/// </summary>
public record LineaPedidoDto(
    Guid IdLineaPedido,
    Guid IdSku,
    string? ClItem,
    string? NbProducto,
    string? NbCombinacion,
    string? NbTalla,
    int NoCantidad,
    decimal MnPrecioUnitario,
    decimal MnDescuentoLinea,
    decimal MnSubtotal
);

/// <summary>
/// Dirección de envío snapshot para la vista del pedido.
/// </summary>
public record DireccionEnvioDto(
    Guid IdDireccion,
    string? NbAlias,
    string DsLinea1,
    string? DsLinea2,
    string NbCiudad,
    string? NbEstado,
    string? ClCodigoPostal,
    string ClPais
);

/// <summary>
/// Entrada del historial de transiciones del pedido.
/// </summary>
public record HistorialPedidoDto(
    string? EstatusAnterior,
    string EstatusNuevo,
    string NbUsuario,
    string? Notas,
    DateTimeOffset RegistradoEn
);
