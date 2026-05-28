namespace BankingApp.Infrastructure.Persistence.Tests.Integration.Persistence.Migrations;

using TestSupport;

[Collection(IntegrationTestCollectionNames.Name)]
public sealed class AppDbContextMigrationsTests(SqlServerDatabaseFixture databaseFixture)
    : IntegrationTestBase(databaseFixture)
{
    [Fact(Skip = "Not implemented yet.")]
    public async Task CreateDbContext_WhenDatabaseHasBeenMigrated_ShouldConnectSuccessfully()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task ResetAsync_WhenDatabaseContainsRows_ShouldRemoveAllPersistedData()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task CreateDbContext_WhenCalledMultipleTimes_ShouldReturnIndependentContexts()
    {
        throw new NotImplementedException();
    }
}
