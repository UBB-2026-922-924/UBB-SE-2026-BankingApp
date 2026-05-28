#!/usr/bin/env python3
"""Unified setup utility for BankingApp development and production configuration."""

from __future__ import annotations

import argparse
import base64
import json
import os
import secrets
import shutil
import string
import subprocess
import sys
from pathlib import Path

SCRIPT_DIR = Path(__file__).resolve().parent
APP_ROOT = SCRIPT_DIR.parent
API_PROJECT = APP_ROOT / "src" / "BankingApp.Api"
WEB_PROJECT = APP_ROOT / "src" / "BankingApp.Web"
DESKTOP_PROJECT = APP_ROOT / "src" / "BankingApp.Desktop"
PRESENTATION_PROJECTS = {
    "API": API_PROJECT,
    "Web": WEB_PROJECT,
    "Desktop": DESKTOP_PROJECT,
}
COMPOSE_ENV_FILE = APP_ROOT / ".env"
API_ENV_FILE = API_PROJECT / ".env"
API_APPSETTINGS_FILE = API_PROJECT / "appsettings.json"
PROD_ENV_FILE = API_PROJECT / ".env.production.generated"
DESKTOP_DEV_APPSETTINGS = DESKTOP_PROJECT / "appsettings.Development.json"
WEB_DEV_APPSETTINGS = WEB_PROJECT / "appsettings.Development.json"

CLIENT_DEV_LOGIN_KEYS = frozenset({"DevLogin:Email", "DevLogin:Password"})

SQL_PASSWORD_LENGTH = 24
JWT_SECRET_BYTES = 48
OTP_SECRET_BYTES = 32

CONNECTION_STRING_KEY = "ConnectionStrings:BankingAppDb"
CONNECTION_STRING_ENV_KEY = "ConnectionStrings__BankingAppDb"

DOCKER_API_CONNECTION_STRING = (
    "Server=db;Database=BankingAppDb;User Id=sa;"
    "Password={password};TrustServerCertificate=True;"
)
DOCKER_DB_CONNECTION_STRING = (
    "Server=localhost,1433;Database=BankingAppDb;User Id=sa;"
    "Password={password};TrustServerCertificate=True;"
)
LOCAL_CONNECTION_STRING = (
    "Server=localhost;Database=BankingAppDb;"
    "Trusted_Connection=True;TrustServerCertificate=True;"
)

DEFAULT_DEV_LOGIN_FULL_NAME = "Development User"

SECRET_KEY_PARTS = (
    "Password",
    "Pass",
    "Secret",
    "Token",
    "ConnectionString",
    "ConnectionStrings",
    "DB_SA_PASSWORD",
)

API_ENV_KEYS = (
    "ASPNETCORE_ENVIRONMENT",
    CONNECTION_STRING_ENV_KEY,
    "Jwt__Secret",
    "Otp__Secret",
    "Database__ApplyMigrations",
    "DevLogin__Email",
    "DevLogin__Password",
    "DevLogin__FullName",
)

DEV_SUMMARY_KEYS = (
    CONNECTION_STRING_KEY,
    "Jwt:Secret",
    "Otp:Secret",
    "Database:ApplyMigrations",
    "DevLogin:Email",
    "DevLogin:Password",
    "DevLogin:FullName",
)

PROD_SUMMARY_KEYS = (
    CONNECTION_STRING_KEY,
    "Jwt:Secret",
    "Otp:Secret",
    "Database:ApplyMigrations",
)


def generate_sql_password(length: int = SQL_PASSWORD_LENGTH) -> str:
    """Generate a SQL Server-compatible password."""
    special_chars = "!@#$%^&*"
    alphabet = string.ascii_letters + string.digits + special_chars

    while True:
        password = "".join(secrets.choice(alphabet) for _ in range(length))
        if (
            any(char.isupper() for char in password)
            and any(char.islower() for char in password)
            and any(char.isdigit() for char in password)
            and any(char in special_chars for char in password)
        ):
            return password


def generate_base64_secret(byte_count: int) -> str:
    """Generate a base64-encoded random secret."""
    return base64.b64encode(secrets.token_bytes(byte_count)).decode("ascii")


def to_env_key(key: str) -> str:
    """Convert an ASP.NET configuration key to an environment variable key."""
    return key.replace(":", "__")


def to_config_key(key: str) -> str:
    """Convert an environment variable key to an ASP.NET configuration key."""
    return key.replace("__", ":")


def is_secret_key(key: str) -> bool:
    """Return whether a key should be masked by default."""
    return any(part.lower() in key.lower() for part in SECRET_KEY_PARTS)


def mask_value(key: str, value: str, show_secrets: bool) -> str:
    """Mask secrets unless explicitly requested."""
    if show_secrets or not is_secret_key(key):
        return value
    if not value:
        return value
    return "***"


def is_placeholder_value(value: str) -> bool:
    """Return whether a config value is only documentation, not a real configured value."""
    return value.startswith("SET-VIA-")


def read_env_file(path: Path) -> dict[str, str]:
    """Read a simple KEY=VALUE env file."""
    values: dict[str, str] = {}
    if not path.exists():
        return values

    for line in path.read_text(encoding="utf-8-sig").splitlines():
        stripped = line.strip()
        if not stripped or stripped.startswith("#") or "=" not in stripped:
            continue
        key, _, value = stripped.partition("=")
        values[key.strip()] = value.strip()

    return values


def write_env_file(path: Path, values: dict[str, str], header: list[str], force: bool) -> None:
    """Write an env file with stable key ordering."""
    if path.exists() and not force:
        raise SystemExit(f"{path} already exists. Re-run with --force to overwrite it.")

    path.parent.mkdir(parents=True, exist_ok=True)
    lines = [*header, ""]
    lines.extend(f"{key}={value}" for key, value in values.items())
    lines.append("")
    path.write_text("\n".join(lines), encoding="utf-8")


def update_env_file(path: Path, key: str, value: str) -> None:
    """Set a single key in a simple env file while preserving other keys."""
    values = read_env_file(path)
    values[key] = value
    write_env_file(path, values, [f"# Updated by {SCRIPT_DIR.name}/setup.py. Do not commit."], force=True)


def read_json_config(path: Path) -> dict[str, object]:
    """Read a JSON appsettings-style file."""
    if not path.exists():
        return {}

    try:
        data = json.loads(path.read_text(encoding="utf-8-sig"))
    except json.JSONDecodeError as exception:
        raise SystemExit(f"Failed to parse {path}: {exception}") from exception

    if not isinstance(data, dict):
        return {}
    return data


def read_json_config_key(path: Path, key: str) -> str | None:
    """Read a colon-delimited key from an appsettings JSON file."""
    current: object = read_json_config(path)
    for part in key.split(":"):
        if not isinstance(current, dict) or part not in current:
            return None
        current = current[part]

    return current if isinstance(current, str) and current else None


def write_json_config_key(path: Path, key: str, value: str) -> None:
    """Set a colon-delimited key in an appsettings JSON file without disturbing other keys."""
    data: dict[str, object] = dict(read_json_config(path)) if path.exists() else {}
    parts = key.split(":")
    node: dict[str, object] = data
    for part in parts[:-1]:
        child = node.get(part)
        if not isinstance(child, dict):
            child = {}
            node[part] = child
        node = child
    node[parts[-1]] = value
    path.write_text(json.dumps(data, indent=2) + "\n", encoding="utf-8")


def dotnet_required() -> None:
    """Fail if dotnet is unavailable."""
    if shutil.which("dotnet") is None:
        raise SystemExit("dotnet was not found on PATH.")


def read_user_secrets(project: Path = API_PROJECT) -> dict[str, str]:
    """Read .NET user secrets for a project."""
    if shutil.which("dotnet") is None:
        return {}

    result = subprocess.run(
        ["dotnet", "user-secrets", "list", "--project", str(project)],
        capture_output=True,
        text=True,
        check=False,
    )
    if result.returncode != 0:
        return {}

    values: dict[str, str] = {}
    for line in result.stdout.splitlines():
        key, separator, value = line.partition(" = ")
        if separator:
            values[key.strip()] = value.strip()

    return values


def set_user_secret(key: str, value: str, project: Path = API_PROJECT) -> None:
    """Set one .NET user secret for a project."""
    dotnet_required()
    result = subprocess.run(
        ["dotnet", "user-secrets", "set", "--project", str(project), key, value],
        capture_output=True,
        text=True,
        check=False,
    )
    if result.returncode != 0:
        message = result.stderr.strip() or result.stdout.strip()
        raise SystemExit(f"Failed to set user secret {key} for {project.relative_to(APP_ROOT)}: {message}")


def remove_user_secret(key: str, project: Path = API_PROJECT) -> None:
    """Remove one .NET user secret for a project if it exists."""
    if shutil.which("dotnet") is None:
        print(f"Skipped user secret {key} for {project.relative_to(APP_ROOT)}: dotnet was not found on PATH.")
        return

    result = subprocess.run(
        ["dotnet", "user-secrets", "remove", "--project", str(project), key],
        capture_output=True,
        text=True,
        check=False,
    )
    if result.returncode != 0:
        message = result.stderr.strip() or result.stdout.strip()
        print(f"Skipped user secret {key} for {project.relative_to(APP_ROOT)}: {message}")


def remove_file(path: Path) -> None:
    """Remove a generated file if it exists."""
    display_path = path
    try:
        display_path = path.resolve().relative_to(APP_ROOT)
    except ValueError:
        display_path = path.resolve()

    if path.exists():
        path.unlink()
        print(f"Removed {display_path}")
        return

    print(f"Not found: {display_path}")


def get_environment_name() -> str:
    """Return the API environment name used for appsettings.{Environment}.json."""
    return (
        os.environ.get("ASPNETCORE_ENVIRONMENT")
        or os.environ.get("DOTNET_ENVIRONMENT")
        or "Production"
    )


def write_presentation_user_secret(key: str, value: str) -> None:
    """Write one .NET user secret to every presentation project."""
    for project in PRESENTATION_PROJECTS.values():
        set_user_secret(key, value, project)


def write_dev_login_to_presentation_user_secrets(email: str, password: str, full_name: str) -> None:
    """Write development login values to presentation-layer .NET user secrets."""
    write_presentation_user_secret("DevLogin:Email", email)
    write_presentation_user_secret("DevLogin:Password", password)
    set_user_secret("DevLogin:FullName", full_name, API_PROJECT)


def build_dev_api_values(args: argparse.Namespace, connection_string: str) -> dict[str, str]:
    """Build API runtime values for development."""
    return {
        "ASPNETCORE_ENVIRONMENT": "Development",
        CONNECTION_STRING_ENV_KEY: connection_string,
        "Jwt__Secret": generate_base64_secret(JWT_SECRET_BYTES),
        "Otp__Secret": generate_base64_secret(OTP_SECRET_BYTES),
        "Database__ApplyMigrations": "true",
        "DevLogin__Email": args.dev_login_email,
        "DevLogin__Password": args.dev_login_password,
        "DevLogin__FullName": args.dev_login_full_name,
    }


def write_user_secrets_from_env_values(values: dict[str, str], project: Path = API_PROJECT) -> None:
    """Write env-style values into .NET user secrets."""
    for key, value in values.items():
        set_user_secret(to_config_key(key), value, project)


def generate_dev(args: argparse.Namespace) -> None:
    """Generate development configuration."""

    existing_compose_env = read_env_file(COMPOSE_ENV_FILE)
    db_password = (
        args.db_password
        or existing_compose_env.get("DB_SA_PASSWORD")
        or generate_sql_password()
    )

    if args.local:
        mode = "local"
        connection_string = LOCAL_CONNECTION_STRING
        write_compose_env = False
        write_api_env = False
        write_user_secrets = True
        next_step = "Run the API from Rider or dotnet run with a local SQL Server."
    elif args.docker_db:
        mode = "docker-db"
        connection_string = DOCKER_DB_CONNECTION_STRING.format(password=db_password)
        write_compose_env = True
        write_api_env = False
        write_user_secrets = True
        next_step = "Run docker compose up db, then run the API from Rider or dotnet run."
    else:
        mode = "docker-api"
        connection_string = DOCKER_API_CONNECTION_STRING.format(password=db_password)
        write_compose_env = True
        write_api_env = True
        write_user_secrets = False
        next_step = "Run docker compose up --build."

    api_values = build_dev_api_values(args, connection_string)
    write_dev_login_to_presentation_user_secrets(
        args.dev_login_email,
        args.dev_login_password,
        args.dev_login_full_name,
    )

    if write_compose_env:
        write_env_file(
            COMPOSE_ENV_FILE,
            {"DB_SA_PASSWORD": db_password},
            ["# Generated by scripts/setup.py for development Docker infrastructure. Do not commit."],
            args.force,
        )

    if write_api_env:
        write_env_file(
            API_ENV_FILE,
            api_values,
            ["# Generated by scripts/setup.py for the development API container. Do not commit."],
            args.force,
        )

    if write_user_secrets:
        write_user_secrets_from_env_values(api_values)

    print(f"Generated development setup: {mode}")
    if write_compose_env:
        print(f"  {COMPOSE_ENV_FILE.relative_to(APP_ROOT)}")
    if write_api_env:
        print(f"  {API_ENV_FILE.relative_to(APP_ROOT)}")
    print("  .NET User Secrets for src/BankingApp.Api")
    print("  .NET User Secrets for src/BankingApp.Web")
    print("  .NET User Secrets for src/BankingApp.Desktop")
    print("\nNext step:")
    print(f"  {next_step}")


def generate_prod(args: argparse.Namespace) -> None:
    """Generate production configuration."""
    if not args.connection_string:
        raise SystemExit("--connection-string is required with --prod.")

    output = args.output.resolve()

    values = {
        "ASPNETCORE_ENVIRONMENT": "Production",
        CONNECTION_STRING_ENV_KEY: args.connection_string,
        "Jwt__Secret": generate_base64_secret(JWT_SECRET_BYTES),
        "Otp__Secret": generate_base64_secret(OTP_SECRET_BYTES),
        "Database__ApplyMigrations": "true" if args.apply_migrations else "false",
    }

    write_env_file(
        output,
        values,
        [
            "# Generated by scripts/setup.py for production.",
            "# Install these values into the production secret store. Do not commit this file.",
        ],
        args.force,
    )

    print("Generated production env file:")
    print(f"  {output}")
    print("\nInstall these values into the server, Docker, CI/CD, or hosting secret store.")
    print("Do not commit the generated file.")


def handle_generate(args: argparse.Namespace) -> None:
    """Dispatch generate command."""
    if args.environment == "prod":
        generate_prod(args)
        return

    generate_dev(args)


def get_sources_for_key(key: str, environment: str) -> list[tuple[str, str | None]]:
    """Return known sources for a config key in increasing precedence order."""
    config_key = to_config_key(key)
    env_key = to_env_key(config_key)
    appsettings_environment_file = API_PROJECT / f"appsettings.{environment}.json"
    sources = [
        (str(API_APPSETTINGS_FILE.relative_to(APP_ROOT)), read_json_config_key(API_APPSETTINGS_FILE, config_key)),
        (str(appsettings_environment_file.relative_to(APP_ROOT)), read_json_config_key(appsettings_environment_file, config_key)),
    ]

    if environment == "Development" and config_key in CLIENT_DEV_LOGIN_KEYS:
        web_user_secrets = read_user_secrets(WEB_PROJECT)
        desktop_user_secrets = read_user_secrets(DESKTOP_PROJECT)
        sources.extend([
            (str(DESKTOP_DEV_APPSETTINGS.relative_to(APP_ROOT)), read_json_config_key(DESKTOP_DEV_APPSETTINGS, config_key)),
            (str(WEB_DEV_APPSETTINGS.relative_to(APP_ROOT)), read_json_config_key(WEB_DEV_APPSETTINGS, config_key)),
            ("Desktop user secrets", desktop_user_secrets.get(config_key)),
            ("Web user secrets", web_user_secrets.get(config_key)),
        ])

    if environment == "Development":
        user_secrets = read_user_secrets(API_PROJECT)
        api_env = read_env_file(API_ENV_FILE)
        compose_env = read_env_file(COMPOSE_ENV_FILE)
        sources.extend(
            [
                (str(COMPOSE_ENV_FILE.relative_to(APP_ROOT)), compose_env.get(env_key) or compose_env.get(key)),
                (str(API_ENV_FILE.relative_to(APP_ROOT)), api_env.get(env_key) or api_env.get(key)),
                ("API user secrets", user_secrets.get(config_key)),
            ]
        )
    else:
        prod_env = read_env_file(PROD_ENV_FILE)
        sources.append((str(PROD_ENV_FILE.relative_to(APP_ROOT)), prod_env.get(env_key) or prod_env.get(key)))

    sources.append((env_key, os.environ.get(env_key)))
    return sources


def get_configured_matches(key: str, environment: str) -> list[tuple[str, str]]:
    """Return non-placeholder configured values for a config key."""
    return [
        (source, value)
        for source, value in get_sources_for_key(key, environment)
        if value and not is_placeholder_value(value)
    ]


def print_setup_summary(environment: str, show_secrets: bool) -> None:
    """Print the important generated setup values for an environment."""
    keys = PROD_SUMMARY_KEYS if environment == "Production" else DEV_SUMMARY_KEYS
    print(f"{environment} setup:")

    for key in keys:
        matches = get_configured_matches(key, environment)
        if not matches:
            print(f"  {key}: not configured")
            continue

        source, value = matches[-1]
        print(f"  {key}: {mask_value(key, value, show_secrets)} ({source})")

    if environment == "Development":
        compose_env = read_env_file(COMPOSE_ENV_FILE)
        db_password = compose_env.get("DB_SA_PASSWORD")
        if db_password:
            print(f"  DB_SA_PASSWORD: {mask_value('DB_SA_PASSWORD', db_password, show_secrets)} ({COMPOSE_ENV_FILE.relative_to(APP_ROOT)})")
        else:
            print("  DB_SA_PASSWORD: not configured")


def handle_get(args: argparse.Namespace) -> None:
    """Handle get command."""
    environment = "Production" if args.environment == "prod" else "Development"
    if args.key is None:
        print_setup_summary(environment, args.show_secrets)
        return

    key = to_config_key(args.key)
    sources = get_sources_for_key(key, environment)
    matches = [(source, value) for source, value in sources if value and not is_placeholder_value(value)]

    if args.all:
        all_sources = [(source, value) for source, value in sources if value]
        if not all_sources:
            print(f"{key}: not configured")
            return
        for source, value in all_sources:
            print(f"{source}: {mask_value(key, value, args.show_secrets)}")
        return

    if not matches:
        print(f"{key}: not configured")
        return

    source, value = matches[-1]
    print(f"{key} source: {source}")
    print(f"{key}: {mask_value(key, value, args.show_secrets)}")


def handle_set(args: argparse.Namespace) -> None:
    """Handle set command."""
    key = to_config_key(args.key)
    value = args.value

    if args.environment == "prod":
        update_env_file(args.output.resolve(), to_env_key(key), value)
        print(f"Set {key} in {args.output.resolve()}.")
        return

    target_count = sum((args.user_secrets, args.api_env, args.compose_env))
    if target_count != 1:
        raise SystemExit("Choose exactly one target: --user-secrets, --api-env, or --compose-env.")

    if args.user_secrets:
        if key in CLIENT_DEV_LOGIN_KEYS:
            write_presentation_user_secret(key, value)
            print(f"Set {key} in API, Web, and Desktop user secrets.")
        else:
            set_user_secret(key, value, API_PROJECT)
            print(f"Set {key} in API user secrets.")
    elif args.api_env:
        update_env_file(API_ENV_FILE, to_env_key(key), value)
        print(f"Set {key} in {API_ENV_FILE.relative_to(APP_ROOT)}.")
    else:
        update_env_file(COMPOSE_ENV_FILE, to_env_key(key), value)
        print(f"Set {key} in {COMPOSE_ENV_FILE.relative_to(APP_ROOT)}.")


def clean_dev() -> None:
    """Remove generated development setup."""
    remove_file(COMPOSE_ENV_FILE)
    remove_file(API_ENV_FILE)
    for key in API_ENV_KEYS:
        remove_user_secret(to_config_key(key), API_PROJECT)
    for key in CLIENT_DEV_LOGIN_KEYS:
        remove_user_secret(key, WEB_PROJECT)
        remove_user_secret(key, DESKTOP_PROJECT)


def clean_prod(output: Path = PROD_ENV_FILE) -> None:
    """Remove generated production setup."""
    remove_file(output.resolve())


def handle_clean(args: argparse.Namespace) -> None:
    """Handle clean command."""
    if args.environment == "dev":
        clean_dev()
        return

    if args.environment == "prod":
        clean_prod(args.output)
        return

    clean_dev()
    clean_prod(args.output)


def add_common_generation_options(parser: argparse.ArgumentParser) -> None:
    """Add options shared by dev and production generation."""
    parser.add_argument("--force", action="store_true", help="Overwrite generated files.")


def parse_args() -> argparse.Namespace:
    """Parse command-line arguments."""
    parser = argparse.ArgumentParser(description="BankingApp setup utility.")
    subparsers = parser.add_subparsers(dest="command", required=True)

    generate_parser = subparsers.add_parser("generate", help="Generate setup configuration.")
    generate_subparsers = generate_parser.add_subparsers(dest="environment", required=True)

    generate_dev_parser = generate_subparsers.add_parser("dev", help="Generate development setup.")
    dev_mode_group = generate_dev_parser.add_mutually_exclusive_group()
    dev_mode_group.add_argument("--local", action="store_true", help="API local, DB local. Default.")
    dev_mode_group.add_argument("--docker-db", action="store_true", help="DB in Docker, API local.")
    dev_mode_group.add_argument("--docker-api", action="store_true", help="API and DB in Docker.")
    add_common_generation_options(generate_dev_parser)
    generate_dev_parser.add_argument("--db-password", default=None, help="SQL Server SA password for Docker dev modes.")
    generate_dev_parser.add_argument("--dev-login-email", required=True, help="Development login email to seed.")
    generate_dev_parser.add_argument("--dev-login-password", required=True, help="Development login password to seed.")
    generate_dev_parser.add_argument(
        "--dev-login-full-name",
        default=DEFAULT_DEV_LOGIN_FULL_NAME,
        help="Development login user's full name.",
    )
    generate_dev_parser.set_defaults(func=handle_generate)

    generate_prod_parser = generate_subparsers.add_parser("prod", help="Generate production setup.")
    add_common_generation_options(generate_prod_parser)
    generate_prod_parser.add_argument("--connection-string", required=True, help="Production SQL Server connection string.")
    generate_prod_parser.add_argument(
        "--apply-migrations",
        action="store_true",
        help="Set Database__ApplyMigrations=true for production.",
    )
    generate_prod_parser.add_argument(
        "--output",
        type=Path,
        default=PROD_ENV_FILE,
        help=f"Production output env file. Defaults to {PROD_ENV_FILE}.",
    )
    generate_prod_parser.set_defaults(func=handle_generate)

    get_parser = subparsers.add_parser("get", help="Read a configuration value.")
    get_subparsers = get_parser.add_subparsers(dest="environment", required=True)
    for environment in ("dev", "prod"):
        environment_parser = get_subparsers.add_parser(environment, help=f"Read a {environment} configuration value.")
        environment_parser.add_argument(
            "key",
            nargs="?",
            help="ASP.NET config key, e.g. ConnectionStrings:BankingAppDb. Omit to show setup summary.",
        )
        environment_parser.add_argument("--all", action="store_true", help="Show all configured sources for the key.")
        environment_parser.add_argument(
            "--show-secrets",
            action="store_true",
            help="Print secret values instead of masking them.",
        )
        environment_parser.set_defaults(func=handle_get)

    set_parser = subparsers.add_parser("set", help="Set a configuration value.")
    set_subparsers = set_parser.add_subparsers(dest="environment", required=True)
    set_dev_parser = set_subparsers.add_parser("dev", help="Set a development configuration value.")
    set_dev_parser.add_argument("key", help="ASP.NET config key, e.g. DevLogin:Email.")
    set_dev_parser.add_argument("value", help="Value to write.")
    set_target_group = set_dev_parser.add_mutually_exclusive_group()
    set_target_group.add_argument("--user-secrets", action="store_true", help="Write to API .NET user secrets.")
    set_target_group.add_argument("--api-env", action="store_true", help="Write to src/BankingApp.Api/.env.")
    set_target_group.add_argument("--compose-env", action="store_true", help="Write to root .env.")
    set_dev_parser.set_defaults(func=handle_set)

    set_prod_parser = set_subparsers.add_parser("prod", help="Set a production configuration value.")
    set_prod_parser.add_argument("key", help="ASP.NET config key, e.g. ConnectionStrings:BankingAppDb.")
    set_prod_parser.add_argument("value", help="Value to write.")
    set_prod_parser.add_argument(
        "--output",
        type=Path,
        default=PROD_ENV_FILE,
        help=f"Production env file. Defaults to {PROD_ENV_FILE}.",
    )
    set_prod_parser.set_defaults(func=handle_set)

    clean_parser = subparsers.add_parser("clean", help="Remove generated setup configuration.")
    clean_subparsers = clean_parser.add_subparsers(dest="environment", required=True)
    clean_dev_parser = clean_subparsers.add_parser("dev", help="Remove generated development setup.")
    clean_dev_parser.set_defaults(func=handle_clean)
    clean_prod_parser = clean_subparsers.add_parser("prod", help="Remove generated production setup.")
    clean_prod_parser.add_argument(
        "--output",
        type=Path,
        default=PROD_ENV_FILE,
        help=f"Production env file. Defaults to {PROD_ENV_FILE}.",
    )
    clean_prod_parser.set_defaults(func=handle_clean)
    clean_all_parser = clean_subparsers.add_parser("all", help="Remove generated development and production setup.")
    clean_all_parser.add_argument(
        "--output",
        type=Path,
        default=PROD_ENV_FILE,
        help=f"Production env file. Defaults to {PROD_ENV_FILE}.",
    )
    clean_all_parser.set_defaults(func=handle_clean)

    args = parser.parse_args()
    if args.command == "generate" and args.environment == "dev" and not args.local and not args.docker_db and not args.docker_api:
        args.local = True
    return args


def main() -> None:
    """Run the setup utility."""
    args = parse_args()
    args.func(args)


if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        sys.exit(130)
