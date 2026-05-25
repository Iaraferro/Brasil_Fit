namespace BrasilFit.Infrastructure.ExternalServices.OpenFoodFacts;

public interface IOpenFoodFactsService
{
    Task<OffProduct?> BuscarPorCodigoBarrasAsync(string codigoBarras, CancellationToken ct = default);
    Task<IReadOnlyList<OffProduct>> BuscarPorNomeAsync(string termo, int pagina = 1, int tamanhoPagina = 20, CancellationToken ct = default);
}
