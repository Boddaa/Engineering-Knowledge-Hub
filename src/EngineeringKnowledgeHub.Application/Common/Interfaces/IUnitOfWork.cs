namespace EngineeringKnowledgeHub.Application.Common.Interfaces;

/// <summary>
/// Unit of work and transaction coordinator abstraction for managing persistence consistency
/// and ensuring governance invariants (e.g. atomic publication and snapshot persistence).
/// </summary>
public interface IUnitOfWork
{
    Task ExecuteTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);

    Task<T> ExecuteTransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
