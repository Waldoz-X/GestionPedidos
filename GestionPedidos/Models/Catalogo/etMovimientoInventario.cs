using System;

namespace GestionPedidos.Models;

/// <summary>
/// HISTORIAL / KARDEX DE MOVIMIENTOS DE INVENTARIO.
/// Registra cada entrada, salida, ajuste o venta física de un SKU de forma permanente.
/// </summary>
public class etMovimientoInventario
{
    public Guid IdMovimiento { get; set; }
    public Guid IdSku { get; set; }
    public int NoCantidad { get; set; }
    public string ClTipoMovimiento { get; set; } = null!; // "ENTRADA" | "BAJA" | "AJUSTE" | "VENTA"
    public string? DsMotivo { get; set; }

    // Auditoría
    public required string ClOperadorCrea { get; set; }
    public required string NbArtefactoCrea { get; set; }
    public DateTimeOffset FeCreacion { get; set; } = DateTimeOffset.UtcNow;

    // Navegación
    public etSku Sku { get; set; } = null!;
}
