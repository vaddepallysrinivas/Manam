namespace Manam.Storage.Abstractions;

/// <summary>
/// Generic repository interface for data access patterns
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<int> CreateAsync(T entity, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task<int> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
