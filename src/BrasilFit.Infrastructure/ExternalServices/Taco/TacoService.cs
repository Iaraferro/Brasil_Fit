using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace BrasilFit.Infrastructure.ExternalServices.Taco;

// Integra com a API comunitaria da Tabela Brasileira de Composicao de Alimentos (TACO/UNICAMP).
// BaseAddress configurado em appsettings.json -> ExternalApis:Taco:BaseUrl
public class TacoService : ITacoService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TacoService> _logger;

    // Cache em memoria para evitar multiplas chamadas para o mesmo termo na mesma instancia.
    private static readonly Dictionary<string, (DateTime Expira, IReadOnlyList<TacoAlimento> Resultado)> _cache = new();
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);

    public TacoService(HttpClient httpClient, ILogger<TacoService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TacoAlimento>> BuscarPorNomeAsync(string termo, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(termo)) return [];

        var chave = termo.Trim().ToLowerInvariant();

        if (_cache.TryGetValue(chave, out var cached) && cached.Expira > DateTime.UtcNow)
            return cached.Resultado;

        try
        {
            // GET /foods?search={termo} — retorna array de alimentos filtrados pelo nome
            var url = $"foods?search={Uri.EscapeDataString(termo.Trim())}";
            var alimentos = await _httpClient.GetFromJsonAsync<TacoAlimento[]>(url, ct);

            var resultado = (IReadOnlyList<TacoAlimento>)(alimentos ?? []);
            _cache[chave] = (DateTime.UtcNow.Add(CacheTtl), resultado);
            return resultado;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "TACO API indisponivel para o termo '{Termo}'. Retornando lista vazia.", termo);
            return [];
        }
    }

    public async Task<TacoAlimento?> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<TacoAlimento>($"foods/{id}", ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "TACO API indisponivel para id {Id}.", id);
            return null;
        }
    }
}
