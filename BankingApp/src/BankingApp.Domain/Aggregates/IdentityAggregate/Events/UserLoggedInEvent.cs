namespace BankingApp.Domain.Aggregates.IdentityAggregate.Events;

using Common;

public record UserLoggedInEvent(int IdentityAccountId, DateTime OccurredOnUtc) : IDomainEvent;
