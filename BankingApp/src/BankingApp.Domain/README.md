# BankingApp.Domain

Contains all business concepts and rules with zero dependencies on infrastructure, frameworks, or other application layers.

## Dependency rule

This project has **no references** to Application, Infrastructure, or any heavy framework. The only external dependencies are:

| Package             | Why                                                                                                                             |
|---------------------|---------------------------------------------------------------------------------------------------------------------------------|
| `ErrorOr`           | Result type — forces callers to handle failure without exceptions                                                               |
| `NodaMoney`         | ISO 4217 money and currency with built-in arithmetic and validation                                                             |
| `MediatR.Contracts` | `INotification` marker interface only — lets domain events be dispatched by MediatR without pulling in MediatR's implementation |

Everything else in the solution depends on this project, never the other way around.

## Structure

```
BankingApp.Domain/
├── Aggregates/          # Aggregate roots and their owned entities/events
├── Common/
│   ├── Errors/          # Typed error definitions (ErrorOr)
│   ├── Extensions/      # Extension methods on domain types
│   └── Primitives/      # Base classes: Entity<T>, AggregateRoot<T>, ValueObject
├── Enums/               # Domain enumerations
├── Repositories/        # Repository interfaces (implemented in Infrastructure)
├── Services/            # Pure domain services (stateless logic, no I/O)
├── ValueObjects/        # Custom value objects with factory validation
└── ReferenceData/       # Read-only reference data (Billers, Categories)
```

## Aggregates

Each aggregate is a consistency boundary. Only the aggregate root is referenced from outside; inner entities are accessed through the root.

| Aggregate root       | Owned entities / events                        | Purpose                                      |
|----------------------|------------------------------------------------|----------------------------------------------|
| `User`               | `Notification`, `NotificationPreference`       | Profile, preferences, notification inbox     |
| `IdentityAccount`    | `Session`, `PasswordResetToken`                | Authentication state, sessions, lockout      |
| `Account`            | `Card`, `Transaction`                          | Bank account, balance, transaction ledger    |
| `Transfer`           | —                                              | Outgoing wire/SEPA transfer lifecycle        |
| `ForexTransaction`   | `LockedRate`                                   | Currency exchange between two accounts       |
| `RateAlert`          | —                                              | User-defined FX rate trigger                 |
| `BillPayment`        | —                                              | One-off bill payment to a biller             |
| `RecurringPayment`   | —                                              | Scheduled recurring bill payment             |
| `Beneficiary`        | —                                              | Saved recipient for transfers                |
| `SavedBiller`        | —                                              | Saved biller shortcut for bill payments      |

## Primitives

- **`Entity<TId>`** — base for all entities; holds a typed `Id`.
- **`AggregateRoot<TId>`** — extends `Entity<TId>`; adds a `DomainEvents` collection. Call `Raise(event)` inside aggregate methods to record what happened. The Application layer dispatches these events after persisting.
- **`ValueObject`** — base record for value objects; record equality gives structural comparison for free.

## Value objects

Custom value objects wrap primitives and enforce their invariants at construction time. All use a **private constructor + static factory** pattern so an invalid instance can never exist.

| Type             | Factory                         | Validates                             |
|------------------|---------------------------------|---------------------------------------|
| `Email`          | `Email.Create(string)`          | Non-empty, contains `@` and a domain  |
| `Iban`           | `Iban.Create(string)`           | Length 15–34, correct country prefix  |
| `HashedPassword` | `HashedPassword.Wrap(string)`   | Wraps a pre-hashed string (no-op)     |

## Errors

All domain errors live in `Common/Errors/` and are static `ErrorOr.Error` fields grouped by feature (`AccountErrors`, `TransferErrors`, etc.). Factory methods on aggregates and value objects return `ErrorOr<T>` so the caller is forced to handle both the success and failure paths without exceptions.

## Repositories

Interfaces only — one per aggregate root. Implementations live in `BankingApp.Infrastructure`. Each interface exposes only the queries actually needed by the domain and Application layer:

- `GetByIdAsync` on every repository
- `ListByUserIdAsync` where the aggregate is user-scoped
- `UpdateAsync` / `DeleteAsync` where mutation is required
- Specialised queries (`ListDueAsync` on recurring payments, `ListActiveAsync` on billers)

## Domain services

Stateless functions that perform cross-aggregate calculations or logic that does not naturally belong on a single aggregate. They take and return domain types only.

Currently: `IbanValidationService` (structural IBAN check used by the `Iban` value object).

## Domain events

`IDomainEvent` extends `MediatR.INotification`, so every domain event can be published directly via MediatR's `IPublisher` without any adapter layer.

**Dispatch flow:**

1. An aggregate method calls `Raise(new SomeEvent(...))` — the event is stored in `_domainEvents` on the root.
2. After `SaveChangesAsync()` succeeds, Infrastructure reads `DomainEvents` from all tracked aggregates, calls `IPublisher.Publish(@event)` for each, then clears the list.
3. Application provides the `INotificationHandler<TEvent>` implementations that react to each event.

**Events by aggregate:**

| Event                            | Raised by                        |
|----------------------------------|----------------------------------|
| `BalanceUpdatedEvent`            | `Account.ChangeBalance`          |
| `TransactionRecordedEvent`       | `Account.RecordTransaction`      |
| `TransferExecutedEvent`          | Application handler after commit |
| `TransferFailedEvent`            | Application handler after commit |
| `BillPaymentProcessedEvent`      | Application handler after commit |
| `RecurringPaymentsExecutedEvent` | Application handler after commit |
| `ForexTransactionExecutedEvent`  | Application handler after commit |
| `RateAlertTriggeredEvent`        | Application handler after commit |
| `UserRegisteredEvent`            | Application handler after commit |
| `UserLoggedInEvent`              | Application handler after commit |
| `PasswordResetRequestedEvent`    | Application handler after commit |
