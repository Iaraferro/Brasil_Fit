using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace BrasilFit.Infrastructure.ExternalServices.OpenFoodFacts;

// HttpClient tipado para a API publica do OpenFoodFacts.
// BaseAddress configurado no Program.cs - aqui usamos apenas paths relativos.
public class OpenFoodFactsService : IOpenFoodFactsService
{

    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenFoodFactsService> _logger;

    public OpenFoodFactsService(HttpClient httpClient, ILogger<OpenFoodFactsService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // 🔧 ADICIONAR O USER-AGENT (OBRIGATÓRIO pela API)
        // O User-Agent será configurado no Program.cs, mas podemos garantir aqui
        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BrasilFit - App de Nutrição - contato@brasilfit.com");
        }
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<OffProduct?> BuscarPorCodigoBarrasAsync(string codigoBarras, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(codigoBarras))
            return null;

        try
        {
            // Endpoint: /api/v2/product/{barcode}.json (v2 funciona melhor)
            var response = await _httpClient.GetAsync($"api/v2/product/{Uri.EscapeDataString(codigoBarras)}.json", ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var dados = await response.Content.ReadFromJsonAsync<OffProductResponse>(cancellationToken: ct);

            // status = 1 => produto encontrado; 0 => nao encontrado.
            if (dados is null || dados.Status != 1 || dados.Product is null)
                return null;

            return dados.Product;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Falha ao consultar OpenFoodFacts por codigo de barras {Codigo}.", codigoBarras);
            // Em vez de lançar exceção, retornar null para não quebrar a API
            return null;
        }
    }

    public async Task<(IReadOnlyList<OffProduct> Produtos, int Total)> BuscarPorNomeAsync(
        string termo, int pagina = 1, int tamanhoPagina = 20, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return (Array.Empty<OffProduct>(), 0);

        if (pagina < 1) pagina = 1;
        if (tamanhoPagina is < 1 or > 100) tamanhoPagina = 20;

        try
        {
            // Endpoint de busca: /cgi/search.pl?search_terms=...&search_simple=1&json=1
            var url = $"cgi/search.pl?search_terms={Uri.EscapeDataString(termo)}" +
                      $"&search_simple=1&action=process&json=1&page={pagina}&page_size={tamanhoPagina}";

            var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var dados = await response.Content.ReadFromJsonAsync<OffSearchResponse>(cancellationToken: ct);
            if (dados is null) return (Array.Empty<OffProduct>(), 0);

            return (dados.Products, dados.Count);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Falha ao buscar produtos no OpenFoodFacts pelo termo {Termo}.", termo);
            // Em vez de lançar exceção, retornar lista vazia
            return (Array.Empty<OffProduct>(), 0);
        }
    }
}
