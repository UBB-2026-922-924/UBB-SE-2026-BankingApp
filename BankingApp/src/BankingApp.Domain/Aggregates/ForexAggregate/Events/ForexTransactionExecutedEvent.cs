namespace BankingApp.Domain.Aggregates.ForexAggregate.Events;

using Common;

public record ForexTransactionExecutedEvent(int ForexTransactionId, DateTime OccurredOnUtc) : IDomainEvent;
