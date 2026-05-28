# BankingApp.Infrastructure.Persistence

Implements persistence and security primitives for Application contracts.

Business rules live in Domain. 

Use-case orchestration lives in Application.

## Dependency rule

Depends on `BankingApp.Application` and `BankingApp.Domain`.

| Package                              | Why                                                      |
|--------------------------------------|----------------------------------------------------------|
| `Microsoft.EntityFrameworkCore`      | ORM for all persistence                                  |
| `BCrypt.Net-Next`                    | Password hashing in `HashService`                        |
| `System.IdentityModel.Tokens.Jwt`    | JWT generation and validation                            |
| `Google.Apis.Auth`                   | Google ID token validation for OAuth login               |
| `Serilog`                            | Structured logging implementation                        |
| `ErrorOr`                            | Result type, consistent with Application and Domain      |

## Structure

```
BankingApp.Infrastructure.Persistence/
├── DependencyInjection/
├── Common/
│   ├── Security/
│   └── Logging/
└── Persistence/
    ├── AppDbContext.cs
    ├── UnitOfWork.cs
    ├── Configurations/   # One IEntityTypeConfiguration<T> per aggregate root
    ├── Migrations/
    └── Repositories/     # One IXRepository implementation per Domain aggregate root
```

## Persistence

### AppDbContext

Inherits `DbContext`. Entity configurations are applied via `ApplyConfigurationsFromAssembly` — `OnModelCreating` contains no inline configuration. Each aggregate root has a corresponding `IEntityTypeConfiguration<T>` in `Configurations/`.

### Repositories

One repository per aggregate root, implementing the `Domain.Repositories.IXRepository` interface. All methods are async. Repositories receive `AppDbContext` via constructor injection and do not wrap a secondary data-access layer.

### UnitOfWork

Wraps `AppDbContext.SaveChangesAsync`. Application handlers call `IUnitOfWork.SaveChangesAsync` once per command after all mutations.

### Migrations

EF Core code-first migrations. Run `dotnet ef migrations add` from this project when the domain model changes.

## Common services

| Class                 | Contract               | Notes                                                      |
|-----------------------|------------------------|------------------------------------------------------------|
| `HashService`         | `IHashService`         | BCrypt verify/hash                                         |
| `JsonWebTokenService` | `IJsonWebTokenService` | Issues and validates JWTs; reads config from `JwtSettings` |

## Registration

```csharp
services.AddPersistenceInfrastructure(configuration);
```

Registers `AppDbContext`, `UnitOfWork`, all repositories, authentication/authorization, and security services.

Cross-cutting infrastructure such as `ISystemClock`, `ILockedRateCache`, and `IExchangeRateService` is registered by `BankingApp.Infrastructure.Core`.
