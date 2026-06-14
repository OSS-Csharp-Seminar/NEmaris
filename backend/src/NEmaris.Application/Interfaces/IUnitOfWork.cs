namespace NEmaris.Application.Interfaces;

public interface IUnitOfWork
{
    Task<T> InSerializableTransactionAsync<T>(Func<Task<T>> action, CancellationToken ct = default);
    Task InSerializableTransactionAsync(Func<Task> action, CancellationToken ct = default);
}
