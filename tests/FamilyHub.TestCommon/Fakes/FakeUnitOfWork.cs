using FamilyHub.Common.Application;

namespace FamilyHub.TestCommon.Fakes;

public class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }
    public int CommitCallCount { get; private set; }
    public int RollbackCallCount { get; private set; }
    public bool HasChanges => false;
    public bool HasActiveTransaction { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        return Task.FromResult(1);
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        HasActiveTransaction = true;
        return Task.CompletedTask;
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        CommitCallCount++;
        HasActiveTransaction = false;
        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        RollbackCallCount++;
        HasActiveTransaction = false;
        return Task.CompletedTask;
    }

    public void Reset()
    {
        SaveChangesCallCount = 0;
        CommitCallCount = 0;
        RollbackCallCount = 0;
        HasActiveTransaction = false;
    }
}
