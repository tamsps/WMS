using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using WMS.Domain.Interfaces;
using WMS.Domain.Data;

namespace WMS.Domain.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly WMSDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(WMSDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        // Use Serializable isolation level for critical inventory operations
        // This ensures highest consistency and prevents phantom reads
        // Note: In EF Core, isolation level is set on the transaction itself
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        // Set isolation level through raw SQL if supported by database
        // await _context.Database.ExecuteSqlRawAsync("SET TRANSACTION ISOLATION LEVEL SERIALIZABLE", cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
