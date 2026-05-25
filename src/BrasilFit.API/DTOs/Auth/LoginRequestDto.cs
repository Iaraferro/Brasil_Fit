using System.ComponentModel.DataAnnotations;

namespace BrasilFit.API.DTOs.Auth;

public class LoginRequestDto
{
    [Required, EmailAddress, MaxLength(180)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6), MaxLength(100)]
    public string Senha { get; set; } = string.Empty;
}
