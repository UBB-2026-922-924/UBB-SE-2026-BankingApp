# Contributing

## Development setup

1. Install the .NET 10 SDK.
2. Install Docker Desktop if you need to run the API with its local dependencies (other ways exist, but I find this to be the easiest).
3. Use Windows 10/11 and the `x64` platform when running the WinUI desktop client.
4. From `BankingApp`, generate local development configuration:

```powershell
python scripts/secrets/setup_dev.py
```

5. Start the local API container if needed:

```powershell
docker compose up --build
```

6. Open `BankingApp/BankingApp.slnx` in Visual Studio/Rider or build it using the `dotnet cli`.

## Running tests

Run all tests from the repository root:

```powershell
dotnet test BankingApp\BankingApp.slnx
```

Run a focused test project while developing:

```powershell
dotnet test BankingApp\tests\BankingApp.Application.Tests\BankingApp.Application.Tests.csproj
dotnet test BankingApp\tests\BankingApp.Api.Tests\BankingApp.Api.Tests.csproj
dotnet test BankingApp\tests\BankingApp.Desktop.Tests\BankingApp.Desktop.Tests.csproj
```

Integration tests may require Docker services and local configuration to be running. Do not merge changes that leave failing tests, formatting violations, or analyzer warnings in touched projects.
Also there are buttons in IDE's for running tests, commands are not necessarly needed.

## Code style

The source of truth for baseline C# formatting and analyzer behavior is the shared build configuration in `BankingApp/Directory.Build.props` together with `BankingApp/.editorconfig`.
The repository uses the SDK .NET analyzers plus `Roslynator.Analyzers`, and warnings are treated as errors during normal builds.
If this document and the analyzer or editor configuration conflict, update the configuration in the same pull request with a clear reason.

1. Use PascalCase for types, methods, properties, events, enum values, constants, and public fields.
2. Use camelCase for local variables and method parameters.
3. Prefix private instance fields with `_` and use camelCase after the prefix, for example `_apiClient`.
4. Prefix interface names with `I`, for example `IApiClient`.
5. Use file-scoped namespaces and place `using` directives inside the namespace.
6. Order `using` directives alphabetically, with `System` namespaces first.
7. Prefer `var` only when the exact type is obvious from the same line through the constructed value, such as `new SomeType(...)`. Use the explicit type for built-in literals and in every other case where the right-hand side does not make the type immediately clear.
8. Keep one public type per `.cs` file, and name the file after that type.
9. Always declare access modifiers explicitly.
10. Keep fields private, exceptions can occur (framework or contract requierments).
11. Prefer `readonly` fields for dependencies assigned in constructors.
12. Use dependency injection instead of constructing services, repositories, clients, loggers, or configuration objects inside application code.
13. Keep controllers thin: validate transport concerns, call application services, and map responses without embedding business rules.
14. Keep domain and application logic independent from API, desktop, database, and UI framework details.
15. Return `ErrorOr<T>` across application service boundaries when an operation can fail in an expected way.
16. Use async APIs for I/O-bound work and name asynchronous methods with the `Async` suffix.
17. Pass `CancellationToken` through public async APIs when the caller can reasonably cancel the operation.
18. Do not use `async void` except for UI event handlers required by WinUI.
19. Prefer `string.Empty` over `""` for empty strings.
20. Use braces for all `if`, `else`, `for`, `foreach`, `while`, and `using` blocks.
21. Add XML documentation to public APIs when the type or member is part of a cross-project contract.
22. Write comments only when they explain intent, business rules, non-obvious tradeoffs, or external constraints.
23. In XAML, use clear `x:Name` values for elements referenced from code-behind, keep bindings explicit, and use observable state (`INotifyPropertyChanged` or observable collections) when UI data changes after load.
24. Name test methods with the `MethodOrScenario_WhenCondition_ExpectedResult` pattern.
25. Structure tests with clear Arrange, Act, and Assert sections.
26. Test expected failures and edge cases, not only the successful path.
27. Prefer null-coalescing expressions over null-check ternaries when the code is equivalent, for example `value ?? fallback` instead of a warning-producing `value is null ? fallback : value`.
28. Keep test data explicit and local to the test unless sharing it removes real duplication without hiding intent.

## Branch naming and Commit messages

Use Conventional Commits. The subject format is:

```text
type(optional-scope): short imperative summary
```

Common types include `feat`, `fix`, `test`, `docs`, `refactor`, `style`, `chore`, `build`, and `ci`. 
Use a scope when it clarifies the affected area, such as `api`, `desktop`, `auth`, `beneficiaries`, or `tests`.

Examples:

```text
feat(beneficiaries): add desktop navigation entry
fix(api): return not found for missing accounts
test: cover password reset validation
docs: add contribution guidelines
```

Keep the first line focused and under roughly 72 characters.
Add a body when the change needs context, such as migration details, tradeoffs, or follow-up work.
Reference issues or PRs when relevant.

Branch names should use the same type vocabulary and a short kebab-case description:

```text
type/something-something
```

Examples:

```text
feat/beneficiaries-page
fix/login-token-refresh
test/account-service-validation
docs/contribution-guidelines
```

## Pull request checklist

Before requesting review, confirm:

- The PR scope matches its issue or clearly explains any intentional scope change.
- Code follows the rules in this document and passes formatting/analyzer checks.
- New or changed behavior is covered by focused tests.
- Existing relevant tests were run, and any tests not run are called out in the PR.
- API changes include request/response DTOs, validation behavior, and error mappings.
- Desktop UI changes are reachable through navigation and update correctly after data changes.
- Async code handles expected failures without unobserved exceptions or deadlocks.
- Public contracts have useful names and XML documentation where appropriate.
- No secrets, local-only settings, generated build output, or unrelated files are included.
- The PR description explains what changed, how it was tested, and any known limitations.
