namespace BankingApp.Infrastructure.Persistence.Tests.Integration.TestSupport;

using Data;

public sealed class TestDbContextFactory(SqlServerDatabaseFixture databaseFixture)
{
    public AppDbContext Create()
    {
        return databaseFixture.CreateDbContext();
    }
}
