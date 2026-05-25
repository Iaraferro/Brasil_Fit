using System.Linq.Expressions;

namespace BrasilFit.Infrastructure.Repositories;

// Contrato generico de acesso. Cuidado: nao abuse - para queries complexas
// crie metodos especificos em repositorios derivados em vez de vazar IQueryable.
public interface IRepository<T> where T : class
{
    Task<T?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListarAsync(Expression<Func<T, bool>>? filtro = null, CancellationToken ct = default);
    Task AdicionarAsync(T entidade, CancellationToken ct = default);
    void Atualizar(T entidade);
    void Remover(T entidade);
    Task<int> SalvarAsync(CancellationToken ct = default);
}
