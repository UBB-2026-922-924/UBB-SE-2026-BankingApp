# BankingApp

Personal banking application.

## Solution Structure

```text
src/
  BankingApp.Domain/          core entities and enums
  BankingApp.Application/     use cases, DTOs, interfaces
  BankingApp.Infrastructure/  persistence and external integrations
  BankingApp.Api/             ASP.NET Core API
  BankingApp.Desktop/         WinUI desktop client
tests/
  BankingApp.Domain.Tests/
  BankingApp.Application.Tests/
  BankingApp.Infrastructure.Tests/
  BankingApp.Infrastructure.Tests.Integration/
  BankingApp.Api.Tests/
  BankingApp.Api.Tests.Integration/
  BankingApp.Desktop.Tests/
```

## Quick Start

### 1. Configure local settings

Generate Docker Compose and API development env files:

```bash
python scripts/setup.py generate dev --dev-login-email dev@example.com --dev-login-password MyPass!1 --local
```

### 2. Start the local stack

```bash
docker compose up --build
```

The API is exposed at `http://localhost:5024`.

### 3. Open the solution

Open `BankingApp.slnx`, set `BankingApp.Desktop` as the startup project, select the `x64` platform, and run it after the API is up.

## Project Docs

- API setup and configuration: [src/BankingApp.Api/README.md](src/BankingApp.Api/README.md)
- Desktop setup and local configuration: [src/BankingApp.Desktop/README.md](src/BankingApp.Desktop/README.md)

The desktop shell now includes a Beneficiaries page in the left navigation, backed by the API and the migrated desktop ViewModel.

## Prerequisites

- Windows 10/11 for the desktop client
- .NET SDK 10.x
- Docker Desktop (optional but highly recommanded)
- Python 3.10+ for local secret generation scripts
