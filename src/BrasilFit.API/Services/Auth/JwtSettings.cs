namespace BrasilFit.API.Services.Auth;

public class JwtSettings
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public int ExpiracaoMinutos { get; set; } = 480; // 8h
}
