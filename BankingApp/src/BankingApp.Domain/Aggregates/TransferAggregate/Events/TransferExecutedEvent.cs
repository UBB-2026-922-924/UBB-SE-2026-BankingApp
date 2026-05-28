namespace BankingApp.Domain.Aggregates.TransferAggregate.Events;

using Common;

public record TransferExecutedEvent(int TransferId, DateTime OccurredOnUtc) : IDomainEvent;
