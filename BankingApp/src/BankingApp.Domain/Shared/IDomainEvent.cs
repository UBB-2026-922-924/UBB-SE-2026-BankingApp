namespace BankingApp.Domain.Common;

public interface IDomainEvent
{
    public DateTime OccurredOnUtc { get; }
}