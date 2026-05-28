# BankingApp Desktop

WinUI 3 desktop client for BankingApp. The app runs unpackaged and talks to the local API over HTTP.

## Prerequisites

- The API must already be running
- Windows 10/11
- .NET SDK 10.x

See [../BankingApp.Api/README.md](../BankingApp.Api/README.md) for API setup.

## Local OAuth Configuration

Create `src/BankingApp.Desktop/appsettings.Local.json` with your Google OAuth credentials.
The file is gitignored and can be edited safely for local development.

## Configuration

Load order:

```text
appsettings.json
appsettings.Local.json
environment variables
```

Key settings:

| Key                        | Default                       |
|----------------------------|-------------------------------|
| `ApiBaseUrl`               | `http://localhost:5024`       |
| `OAuth:Google:Authority`   | `https://accounts.google.com` |
| `OAuth:Google:RedirectUri` | `http://127.0.0.1:7890/`      |

`OAuth:Google:ClientId` and `OAuth:Google:ClientSecret` are required for Google sign-in.

## Running

Open `BankingApp.slnx`, set `BankingApp.Desktop` as the startup project, select `x64`, and run it.

For Rider, create a compound run configuration with `BankingApp.Api` and `BankingApp.Desktop`.

The desktop shell includes a Beneficiaries page in the left navigation, wired to the API-backed view model for loading
and managing saved beneficiaries.
