namespace BrasilFit.Infrastructure.ExternalServices.Taco;

public interface ITacoService
{
    Task<IReadOnlyList<TacoAlimento>> BuscarPorNomeAsync(string termo, CancellationToken ct = default);
    Task<TacoAlimento?> ObterPorIdAsync(int id, CancellationToken ct = default);
}
