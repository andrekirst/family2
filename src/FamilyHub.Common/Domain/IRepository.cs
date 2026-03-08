namespace FamilyHub.Common.Domain;

public interface IReadRepository<TEntity, TId>
    where TEntity : class
    where TId : struct
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default);
    Task<bool> ExistsByIdAsync(TId id, CancellationToken ct = default);
}

public interface IWriteRepository<TEntity, TId>
    : IReadRepository<TEntity, TId>
    where TEntity : class
    where TId : struct
{
    Task AddAsync(TEntity entity, CancellationToken ct = default);
}
