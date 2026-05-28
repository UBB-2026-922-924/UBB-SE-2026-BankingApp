namespace BankingApp.Domain.Aggregates.UserAggregate.Events;

using Common;

public record UserRegisteredEvent(int UserId, DateTime OccurredOnUtc) : IDomainEvent;
