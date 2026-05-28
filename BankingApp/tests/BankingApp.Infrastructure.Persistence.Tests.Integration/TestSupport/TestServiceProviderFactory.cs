namespace BankingApp.Infrastructure.Persistence.Tests.Integration.TestSupport;

using Infrastructure.Persistence.DependencyInjection;

public sealed class TestServiceProviderFactory(string connectionString)
{
    public IServiceProvider Create()
    {
        ServiceCollection services = new();

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:BankingAppDb"] = connectionString,
                ["Jwt:Secret"] = "integration-tests-jwt-secret"
            })
            .Build();

        services.AddPersistenceInfrastructure(configuration);

        return services.BuildServiceProvider(validateScopes: true);
    }
}
