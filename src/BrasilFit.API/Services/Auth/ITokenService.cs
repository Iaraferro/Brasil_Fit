using BrasilFit.Domain.Entities;

namespace BrasilFit.API.Services.Auth;

public interface ITokenService
{
    (string Token, DateTime ExpiraEm) GerarToken(Usuario usuario);
}
