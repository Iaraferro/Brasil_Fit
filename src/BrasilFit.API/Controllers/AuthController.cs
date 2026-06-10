using BrasilFit.API.DTOs.Auth;
using BrasilFit.API.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrasilFit.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    // ENDPOINT PUBLICO #1 - Login para gerar JWT.
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, CancellationToken ct)
    {
        var resposta = await _authService.AutenticarAsync(dto, ct);
        if (resposta is null)
            return Unauthorized(new { mensagem = "Credenciais invalidas." });

        return Ok(resposta);
    }


}
