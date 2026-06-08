using System.ComponentModel.DataAnnotations;

namespace GestionPedidos.Contracts.Auth;

/// <summary>
/// Credenciales de login para obtener un JWT.
/// </summary>
public sealed record LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = null!;

    [Required]
    public string Password { get; init; } = null!;

    /// <summary>
    /// Opcional: especifica el tipo de usuario ("EMPLEADO" o "CLIENTE")
    /// Si no se proporciona, se detecta automáticamente
    /// </summary>
    public string? TipoUsuario { get; init; } = null;
}
