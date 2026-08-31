using CareFlow.Application.DTOs.Auth;

namespace CareFlow.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<UserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default);
}
