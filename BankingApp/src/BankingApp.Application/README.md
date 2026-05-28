# BankingApp.Application

Orchestrates use cases by coordinating domain aggregates, repositories, and external service contracts.

Business rules live in the Domain. I/O implementations live in Infrastructure.

## Dependency rule

Depends only on `BankingApp.Domain`. All I/O is hidden behind interfaces defined here and implemented in Infrastructure.

| Package                                     | Why                                                                      |
|---------------------------------------------|--------------------------------------------------------------------------|
| `MediatR`                                   | CQRS dispatcher, decouples handlers from controllers                     |
| `FluentValidation`                          | Declarative input validation wired into the MediatR pipeline             |
| `ErrorOr`                                   | Result type used in Domain                                               |
| `NodaMoney`                                 | Money arithmetic, also used in Domain                                    |
| `Microsoft.Extensions.Logging.Abstractions` | Logging abstractions only, the actual logging deps are in Infrastructure |

## Structure

```
BankingApp.Application/
├── Common/
│   ├── Behaviors/       # MediatR pipeline: logging, validation
│   ├── Contracts/       # Application-defined service interfaces (implemented in Infrastructure)
│   │   ├── Security/
│   │   └── Notifications/
│   ├── Logging/         # [LoggerMessage] source-generated log messages
│   └── Validation/      # Shared InputRules helpers (email, phone, password)
├── DependencyInjection/
└── Features/
    ├── Commands/        # Write operations
    ├── Queries/         # Read operations
    ├── Dtos/            # Request/response shapes crossing the API boundary
    └── (Validators)     # FluentValidation validators co-located with their command
```

## CQRS pattern

Every operation is either a **command** (mutates state and returns `ErrorOr<T>`) or a **query** (reads state and returns `ErrorOr<T>`). Command and handler are co-located in the same file. Validators follow the same co-location convention when a command needs them.

## Pipeline behaviors

Behaviors run for every MediatR request in registration order:

1. **`ValidationBehavior`** — runs all `IValidator<TRequest>` in parallel, maps failures to `Error.Validation(...)`, short-circuits if any fail.
2. **`LoggingBehavior`** — logs start/end at `Information`, errors at `Error`.

## Contracts

Interfaces that the Application layer depends on but does not implement.

### General contracts

| Interface              | Purpose                                                                |
|------------------------|------------------------------------------------------------------------|
| `IUnitOfWork`          | Wraps `SaveChangesAsync` — called once per command after all mutations |
| `ISystemClock`         | Abstracts `DateTime.UtcNow` for deterministic testing                  |
| `IExchangeRateService` | Returns a live forex rate for a currency pair (synchronous)            |
| `ILockedRateCache`     | Short-lived in-memory rate lock per user for the 2-step forex flow     |

### Security (`Common/Contracts/Security/`)

`IHashService`, `IJsonWebTokenService`

## Features

### Authentication

`LoginCommand` validates credentials, enforces lockout (5 attempts / 15 min), opens a session, and returns a JWT.

Other handlers: `LogoutCommand`

### User Registration

`RegisterCommand` validates email and password strength, creates `User` + `IdentityAccount`.

### User Profile

Profile read/write, password change, session listing and revocation, notification preferences.

### Password Reset

Three-step flow: `ForgotPasswordCommand` → `VerifyResetTokenQuery` → `ResetPasswordCommand`. The token is SHA-256 hashed before storage.

### Account Overview (Dashboard)

`GetDashboardQuery` assembles user summary, all account cards, and the 5 most recent transactions across all accounts.

### Transfers

`ExecuteTransferCommand` validates IBAN and currency, debits the source account, records a ledger transaction, and updates beneficiary stats. Supporting queries: history, account list, IBAN validation, FX preview.

### Forex (Currency Exchange)

Two-step commit. `GetRatePreviewQuery` fetches the live rate, stores a `LockedRate` in `ILockedRateCache`, and returns a `ForexRatePreviewResponse`. `ExecuteForexCommand` retrieves and validates the lock (must match the requested pair, 30 s TTL), debits source, credits target, and clears the cache entry.

### Forex Rate Alerts

CRUD on user-defined rate alerts. `ProcessRateAlertsCommand` is dispatched by the background service — it checks all untriggered alerts against live rates and calls `MarkTriggered()` on matches.

### Bill Payments

`ProcessBillPaymentCommand` applies a tiered fee via `BillPaymentFeePolicy` (≤ 100 → 0.50, > 100 → 1.00), debits the account, and generates a receipt number.

### Billers

CRUD on saved billers (user shortcuts). `GetBillersQuery` returns active billers from reference data.

### Recurring Payments

Lifecycle commands: create, pause, resume, cancel. `ProcessDueRecurringPaymentsCommand` is dispatched by the background service — it processes each due payment using the same fee policy as bill payments and advances the schedule on success.

### Beneficiaries

CRUD on saved transfer recipients. `CreateBeneficiaryCommand` validates the IBAN and rejects duplicates. `ExecuteTransferCommand` updates beneficiary stats (total sent, transfer count) after each successful transfer.

## Background processing

`ProcessDueRecurringPaymentsCommand` and `ProcessRateAlertsCommand` are dispatched by `FinanceBackgroundService` (in the API project). Both return `ErrorOr<int>` (count processed) and are safe to run on every tick.

## Registration

```csharp
services.AddApplication();
```

Registers MediatR, both pipeline behaviors, and all FluentValidation validators from this assembly.
