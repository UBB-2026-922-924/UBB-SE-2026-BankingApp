namespace BankingApp.Domain.Aggregates.AccountAggregate.Events;

using Common;

public sealed record BalanceUpdatedEvent(
    int AccountId,
    decimal OldBalance,
    decimal NewBalance,
    DateTime OccurredOnUtc) : IDomainEvent;
