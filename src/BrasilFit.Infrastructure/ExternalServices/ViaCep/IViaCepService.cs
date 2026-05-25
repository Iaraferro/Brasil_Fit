namespace BrasilFit.Infrastructure.ExternalServices.ViaCep;

public interface IViaCepService
{
    // Retorna null quando o CEP nao for encontrado.
    Task<ViaCepResponse?> BuscarPorCepAsync(string cep, CancellationToken ct = default);
}
