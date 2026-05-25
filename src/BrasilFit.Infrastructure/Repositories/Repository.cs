using System.Linq.Expressions;
using BrasilFit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BrasilFit.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext Context;
    protected readonly DbSet<T> Set;

    public Repository(AppDbContext context)
    {
        Context = context;
        Set = context.Set<T>();
    }

    public Task<T?> ObterPorIdAsync(int id, CancellationToken ct = default)
        => Set.FindAsync(new object?[] { id }, ct).AsTask();

    public async Task<IReadOnlyList<T>> ListarAsync(Expression<Func<T, bool>>? filtro = null, CancellationToken ct = default)
    {
        IQueryable<T> query = Set.AsNoTracking();
        if (filtro is not null)
            query = query.Where(filtro);
        return await query.ToListAsync(ct);
    }

    public async Task AdicionarAsync(T entidade, CancellationToken ct = default)
        => await Set.AddAsync(entidade, ct);

    public void Atualizar(T entidade) => Set.Update(entidade);

    public void Remover(T entidade) => Set.Remove(entidade);

    public Task<int> SalvarAsync(CancellationToken ct = default) => Context.SaveChangesAsync(ct);
}
