using BrasilFit.API.DTOs.Auth;
using BrasilFit.Domain.Entities;
using BrasilFit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BrasilFit.API.Services.Auth;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokenService;

    public AuthService(AppDbContext context, IPasswordHasher hasher, ITokenService tokenService)
    {
        _context = context;
        _hasher = hasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto?> AutenticarAsync(LoginRequestDto dto, CancellationToken ct = default)
    {
        var usuario = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Ativo, ct);

        if (usuario is null) return null;
        if (!_hasher.Verify(dto.Senha, usuario.SenhaHash)) return null;

        var (token, expiraEm) = _tokenService.GerarToken(usuario);

        return new LoginResponseDto
        {
            Token    = token,
            ExpiraEm = expiraEm,
            Nome     = usuario.Nome,
            Email    = usuario.Email,
            Papel    = usuario.Papel.ToString(),
            Id       = usuario.Id,
            Crn      = usuario is Nutricionista n ? n.Crn : null
        };
    }
}
