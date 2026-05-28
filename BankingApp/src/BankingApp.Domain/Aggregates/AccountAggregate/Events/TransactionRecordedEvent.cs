namespace BankingApp.Domain.Aggregates.AccountAggregate.Events;

using Common;

public sealed record TransactionRecordedEvent(
    int AccountId,
    string TransactionRef,
    DateTime OccurredOnUtc) : IDomainEvent;
