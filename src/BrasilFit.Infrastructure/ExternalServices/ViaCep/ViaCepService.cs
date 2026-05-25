using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace BrasilFit.Infrastructure.ExternalServices.ViaCep;

// Implementacao do client tipado HttpClient para ViaCEP.
// O HttpClient e injetado pelo IHttpClientFactory atraves de AddHttpClient<>(...).
public class ViaCepService : IViaCepService
{
    private static readonly Regex CepRegex = new(@"^\d{8}$", RegexOptions.Compiled);
    private readonly HttpClient _httpClient;
    private readonly ILogger<ViaCepService> _logger;

    public ViaCepService(HttpClient httpClient, ILogger<ViaCepService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ViaCepResponse?> BuscarPorCepAsync(string cep, CancellationToken ct = default)
    {
        var cepLimpo = (cep ?? string.Empty).Replace("-", string.Empty).Trim();
        if (!CepRegex.IsMatch(cepLimpo))
        {
            _logger.LogWarning("CEP em formato invalido: {Cep}", cep);
            return null;
        }

        try
        {
            // Endpoint do ViaCEP: https://viacep.com.br/ws/{cep}/json/
            var response = await _httpClient.GetAsync($"ws/{cepLimpo}/json/", ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var dados = await response.Content.ReadFromJsonAsync<ViaCepResponse>(cancellationToken: ct);

            // ViaCEP devolve { "erro": true } quando o CEP nao existe (status 200).
            if (dados is null || dados.Erro == true)
            {
                return null;
            }

            return dados;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Falha ao consultar ViaCEP para o CEP {Cep}.", cepLimpo);
            throw;
        }
    }
}
