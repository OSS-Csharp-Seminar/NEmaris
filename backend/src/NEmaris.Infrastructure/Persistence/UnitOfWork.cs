using System.Data;
using Microsoft.EntityFrameworkCore;
using NEmaris.Application.Interfaces;

namespace NEmaris.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _ctx;

    public UnitOfWork(AppDbContext ctx) => _ctx = ctx;

    public async Task<T> InSerializableTransactionAsync<T>(Func<Task<T>> action, CancellationToken ct = default)
    {
        await using var tx = await _ctx.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var result = await action();
        await tx.CommitAsync(ct);
        return result;
    }

    public async Task InSerializableTransactionAsync(Func<Task> action, CancellationToken ct = default)
    {
        await InSerializableTransactionAsync<object?>(async () =>
        {
            await action();
            return null;
        }, ct);
    }
}
