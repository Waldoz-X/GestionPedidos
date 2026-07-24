namespace GestionPedidos.Contracts.Auth;

/// <summary>
/// Respuesta tras un login exitoso.
/// </summary>
public sealed record LoginResponse
{
    public Guid IdUsuario { get; init; }
    public string AccessToken { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string UserName { get; init; } = null!;
    public string TipoUsuario { get; init; } = null!;  // "EMPLEADO" o "CLIENTE"
    public Guid? IdEmpleado { get; init; }  // null si es cliente
    public Guid? IdCliente { get; init; }   // null si es empleado
    public DateTime ExpiresAt { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];
    public IReadOnlyList<string> Permissions { get; init; } = [];
}
