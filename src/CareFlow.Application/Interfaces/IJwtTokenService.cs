using CareFlow.Domain.Entities;

namespace CareFlow.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user, IEnumerable<string> roles);
    DateTime GetTokenExpiration();
}
