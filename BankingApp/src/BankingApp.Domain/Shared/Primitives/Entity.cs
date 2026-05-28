namespace BankingApp.Domain.Common.Primitives;

public abstract class Entity<TId>
    where TId : notnull
{
    public TId Id { get; protected set; } = default!;
}
