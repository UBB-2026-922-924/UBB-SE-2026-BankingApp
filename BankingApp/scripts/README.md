# BankingApp Setup CLI

Use `scripts/setup.py` to generate, inspect, edit, and clean BankingApp environment setup.

## Commands

```bash
python scripts/setup.py generate dev --dev-login-email <email> --dev-login-password <password> [--local | --docker-db | --docker-api]
python scripts/setup.py get dev [--show-secrets]
python scripts/setup.py generate prod --connection-string "..."
python scripts/setup.py get dev <key>
python scripts/setup.py get prod <key>
python scripts/setup.py set dev <key> <value> [--user-secrets | --api-env | --compose-env]
python scripts/setup.py set prod <key> <value>
python scripts/setup.py clean dev
python scripts/setup.py clean prod
python scripts/setup.py clean all
```

## Dev Output

`generate dev` writes the values needed for local development.

Depending on mode (`--local`, `--docker-db`, `--docker-api`):

```txt
BankingApp/.env                                              (docker-db and docker-api modes)
BankingApp/src/BankingApp.Api/.env                          (docker-api mode)
BankingApp/src/BankingApp.Api user secrets
BankingApp/src/BankingApp.Web user secrets
BankingApp/src/BankingApp.Desktop user secrets
```

`--dev-login-email` and `--dev-login-password` are required. They are written with `dotnet user-secrets` to all three presentation projects (API, Desktop, Web) so every client uses the same dev account.

### Dev login example

```bash
python scripts/setup.py generate dev \
  --dev-login-email dev@example.com \
  --dev-login-password MyPass!1 \
  --local
```

## Prod Output

`generate prod` writes:

```txt
BankingApp/src/BankingApp.Api/.env.production.generated
```

## Configuration Keys

```txt
ConnectionStrings:BankingAppDb
Jwt:Secret
Otp:Secret
Database:ApplyMigrations
DevLogin:Email        (API + Desktop + Web — set by generate dev and set dev)
DevLogin:Password     (API + Desktop + Web — set by generate dev and set dev)
DevLogin:FullName     (API only)
```

## Notes

- `get dev DevLogin:Email` and `get dev DevLogin:Password` show all three targets (API, Desktop, Web) when using `--all`.
- `get dev` shows a masked setup summary. Add `--show-secrets` only when you need to see local credentials such as the SQL Server password.
- `set dev DevLogin:Email <value> --user-secrets` and `set dev DevLogin:Password <value> --user-secrets` write to API, Desktop, and Web user secrets.
- `appsettings.Development.json` files document required keys with `SET-VIA-USER-SECRETS` placeholders; do not put real secrets there.
- `get` reads the active config source for the selected environment.
- `set` updates the selected target for the chosen environment.
- `clean` removes generated setup files and development secrets.
