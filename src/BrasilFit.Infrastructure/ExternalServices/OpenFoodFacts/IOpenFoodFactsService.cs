namespace BrasilFit.Infrastructure.ExternalServices.OpenFoodFacts;

public interface IOpenFoodFactsService
{
    Task<OffProduct?> BuscarPorCodigoBarrasAsync(string codigoBarras, CancellationToken ct = default);

    // Devolve os produtos da pagina e o total reportado pela API,
    // para que o controller consiga montar uma resposta paginada padronizada.
    Task<(IReadOnlyList<OffProduct> Produtos, int Total)> BuscarPorNomeAsync(
        string termo,
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken ct = default);
}
