namespace BankingApp.Domain.Aggregates.BillPaymentAggregate.Events;

using Common;

public record BillPaymentProcessedEvent(int BillPaymentId, DateTime OccurredOnUtc) : IDomainEvent;
