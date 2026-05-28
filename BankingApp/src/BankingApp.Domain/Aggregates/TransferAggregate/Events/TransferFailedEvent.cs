namespace BankingApp.Domain.Aggregates.TransferAggregate.Events;

using Common;

public record TransferFailedEvent(int TransferId, DateTime OccurredOnUtc) : IDomainEvent;
