namespace GestionPedidos.Contracts.Pedidos;

/// <summary>
/// Resumen ejecutivo del estado de los pedidos en el sistema.
/// Exclusivo para Jefes de Departamento, Managers y Administradores.
/// </summary>
public record DashboardResumenDto(
    int TotalPedidosBorrador,
    int TotalPedidosConfirmados,
    int TotalPedidosFacturados,
    int TotalPedidosEnviados,
    int TotalPedidosCancelados,
    decimal MontoPendienteFacturar, // Sumatoria monetaria de pedidos CONFIRMADOS
    decimal MontoTotalFacturado,    // Sumatoria monetaria de pedidos FACTURADOS
    List<PedidoResumenDto> UltimosPedidosConfirmados
);
