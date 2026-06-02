namespace BankingApp.Infrastructure.Persistence.DependencyInjection;

using System.Text;
using Application.Common.Security;
using Application.Shared.Persistence;
using Domain.Repositories;
using Common.Security;
using Data;
using Data.Seeders;
using Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistenceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("BankingAppDb")
                                  ?? throw new InvalidOperationException(
                                      "Configuration value 'ConnectionStrings:BankingAppDb' is missing.");
        string jwtSecret = configuration["Jwt:Secret"]
                           ?? throw new InvalidOperationException("Configuration value 'Jwt:Secret' is missing.");
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                };
            });
        services.AddAuthorization();

        services.AddMemoryCache();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString)
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IIdentityRepository, IdentityRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IBeneficiaryRepository, BeneficiaryRepository>();
        services.AddScoped<IBillerRepository, BillerRepository>();
        services.AddScoped<IBillPaymentRepository, BillPaymentRepository>();
        services.AddScoped<ITransferRepository, TransferRepository>();
        services.AddScoped<IForexRepository, ForexTransactionRepository>();
        services.AddScoped<ISavedBillerRepository, SavedBillerRepository>();

        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IInvestmentRepository, InvestmentRepository>();
        services.AddScoped<ISavingsRepository, SavingsRepository>();
        services.AddScoped<ILoanRepository, LoanRepository>();

        services.AddScoped<IHashService, HashService>();
        services.AddScoped<IJsonWebTokenService>(_ => new JsonWebTokenService(jwtSecret));

        return services;
    }

    /// <summary>
    ///     Seeds reference data (billers, etc.) that must be present on every environment.
    ///     Call this after <c>Database.Migrate()</c> during application startup.
    /// </summary>
    public static async Task SeedReferenceDataAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ILogger<AppDbContext> logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();
        await BillerSeeder.SeedAsync(dbContext, logger, cancellationToken);
    }
}
