using System.Globalization;
using BankingApp.Api.Middleware;
using BankingApp.Application.DependencyInjection;
using BankingApp.Application.Features.Authentication.Services;
using BankingApp.Contracts.Http;
using BankingApp.Domain.Common.Errors;
using BankingApp.Infrastructure.Core.DependencyInjection;
using BankingApp.Infrastructure.Persistence.Data;
using BankingApp.Infrastructure.Persistence.DependencyInjection;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;

const string defaultLogFilePath = "logs/bankingapp-server-.log";
const int retainedLogFileCountLimit = 14;
const int commandLineExecutableArgumentCount = 1;
const int internalServerErrorStatusCode = StatusCodes.Status500InternalServerError;
const string applyDatabaseMigrationsConfigurationKey = "Database:ApplyMigrations";

// Configure Serilog before building the host so that startup errors are also captured.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .WriteTo.File(
        defaultLogFilePath,
        formatProvider: CultureInfo.InvariantCulture,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: retainedLogFileCountLimit)
    .CreateBootstrapLogger();
try
{
    Log.Information("Starting BankingApp.Api");
    string[] commandLineArguments = Environment.GetCommandLineArgs().Skip(commandLineExecutableArgumentCount).ToArray();
    WebApplicationBuilder builder = WebApplication.CreateBuilder(commandLineArguments);
    // Replace the default MEL providers with Serilog. Configuration (log levels, sinks)
    // can be further overridden via appsettings.json under the "Serilog" key.
    builder.Host.UseSerilog((context, services, configurationBuilder) =>
        configurationBuilder
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .WriteTo.File(
                context.Configuration["Logging:FilePath"] ?? defaultLogFilePath,
                formatProvider: CultureInfo.InvariantCulture,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: retainedLogFileCountLimit));
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition(
            AuthHeaderNames.BearerScheme,
            new OpenApiSecurityScheme
            {
                Name = AuthHeaderNames.Authorization,
                Type = SecuritySchemeType.Http,
                Scheme = AuthHeaderNames.BearerScheme,
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Paste your JWT token here",
            });
        options.AddSecurityRequirement(_ =>
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference(referenceId: AuthHeaderNames.BearerScheme),
                    []
                },
            });
    });
    builder.Services.AddApplication();
    builder.Services.AddPersistenceInfrastructure(builder.Configuration);
    builder.Services.AddCoreInfrastructure(builder.Configuration);

    WebApplication application = builder.Build();
    bool applyDatabaseMigrations = !bool.TryParse(
        application.Configuration[applyDatabaseMigrationsConfigurationKey],
        out bool configuredApplyDatabaseMigrations) || configuredApplyDatabaseMigrations;
    if (applyDatabaseMigrations && !application.Environment.IsEnvironment("Testing"))
    {
        using IServiceScope scope = application.Services.CreateScope();
        AppDbContext databaseContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        databaseContext.Database.Migrate();
    }

    if (!application.Environment.IsEnvironment("Testing"))
    {
        await application.Services.SeedReferenceDataAsync();
    }

    if (application.Environment.IsDevelopment())
    {
        await SeedDevelopmentLoginAsync(application);
    }

    application.UseExceptionHandler(exceptionApplicationBuilder => exceptionApplicationBuilder.Run(async context =>
    {
        context.Response.StatusCode = internalServerErrorStatusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = "Something went wrong." });
    }));
    if (application.Environment.IsDevelopment())
    {
        application.UseSwagger();
        application.UseSwaggerUI();
    }

    // Logs each HTTP request: method, path, status code, and duration.
    // Placed before middleware that may short-circuit the pipeline so all
    // requests are captured, including those rejected by session validation.
    application.UseSerilogRequestLogging();
    application.UseAuthentication();
    application.UseMiddleware<SessionValidationMiddleware>();
    application.UseAuthorization();
    application.MapControllers();
    application.Run();
}
catch (HostAbortedException)
{
    throw;
}
catch (Exception exception)
{
    Log.Fatal(exception, "BankingApp.Api terminated unexpectedly");
    throw;
}
finally
{
    // Flush and close all Serilog sinks before the process exits.
    Log.CloseAndFlush();
}

return;

static async Task SeedDevelopmentLoginAsync(WebApplication application)
{
    string? email = application.Configuration["DevLogin:Email"];
    string? password = application.Configuration["DevLogin:Password"];
    string fullName = application.Configuration["DevLogin:FullName"] ?? "Development User";

    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
    {
        Log.Information("Development login seed skipped because DevLogin:Email or DevLogin:Password is not configured.");
        return;
    }

    using IServiceScope scope = application.Services.CreateScope();
    IAuthService authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    ErrorOr<Success> result = await authService.RegisterAsync(email, password, fullName);

    if (!result.IsError)
    {
        Log.Information("Development login user seeded: {Email}", email);
        return;
    }

    if (result.Errors.Any(error => error.Code == AuthErrors.EmailAlreadyRegistered.Code))
    {
        Log.Information("Development login user already exists: {Email}", email);
        return;
    }

    string errors = string.Join(
        "; ",
        result.Errors.Select(error => $"{error.Code}: {error.Description}"));
    throw new InvalidOperationException($"Development login seed failed for {email}: {errors}");
}
