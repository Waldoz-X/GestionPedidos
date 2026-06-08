using GestionPedidos.Models;

namespace GestionPedidos.Security;

public interface IJwtTokenService
{
    string GenerateToken(etUsuario usuario, IEnumerable<string> roles);
}
