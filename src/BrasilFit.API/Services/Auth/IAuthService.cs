using BrasilFit.API.DTOs.Auth;

namespace BrasilFit.API.Services.Auth;

public interface IAuthService
{
    Task<LoginResponseDto?> AutenticarAsync(LoginRequestDto dto, CancellationToken ct = default);
}
