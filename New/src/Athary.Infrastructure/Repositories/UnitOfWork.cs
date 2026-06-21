using Athary.Domain.Interfaces;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Athary.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
            await _currentTransaction.CommitAsync(cancellationToken);
        _currentTransaction?.Dispose();
        _currentTransaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
            await _currentTransaction.RollbackAsync(cancellationToken);
        _currentTransaction?.Dispose();
        _currentTransaction = null;
    }
}
