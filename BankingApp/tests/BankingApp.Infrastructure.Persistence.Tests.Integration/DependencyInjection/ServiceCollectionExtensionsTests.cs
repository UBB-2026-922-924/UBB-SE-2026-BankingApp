namespace BankingApp.Infrastructure.Persistence.Tests.Integration.DependencyInjection;

using TestSupport;

[Collection(IntegrationTestCollectionNames.Name)]
public sealed class ServiceCollectionExtensionsTests(SqlServerDatabaseFixture databaseFixture)
    : IntegrationTestBase(databaseFixture)
{
    [Fact(Skip = "Not implemented yet.")]
    public void AddInfrastructure_WhenConfigurationContainsRequiredSettings_ShouldRegisterCoreInfrastructureServices()
    {
    }

    [Fact(Skip = "Not implemented yet.")]
    public void AddInfrastructure_WhenConfigurationContainsRequiredSettings_ShouldRegisterRepositoryImplementations()
    {
    }

    [Fact(Skip = "Not implemented yet.")]
    public void AddInfrastructure_WhenConnectionStringIsMissing_ShouldThrowInvalidOperationException()
    {
    }

    [Fact(Skip = "Not implemented yet.")]
    public void AddInfrastructure_WhenJwtSecretIsMissing_ShouldThrowInvalidOperationException()
    {
    }

}
