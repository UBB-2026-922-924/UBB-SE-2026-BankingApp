namespace BankingApp.Infrastructure.Persistence.Tests.Integration.Persistence.UnitOfWork;

using TestSupport;

[Collection(IntegrationTestCollectionNames.Name)]
public sealed class UnitOfWorkTests(SqlServerDatabaseFixture databaseFixture)
    : IntegrationTestBase(databaseFixture)
{
    [Fact(Skip = "Not implemented yet.")]
    public async Task SaveChangesAsync_WhenTrackedChangesExist_ShouldPersistChanges()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task SaveChangesAsync_WhenAggregateRaisesDomainEvents_ShouldPublishDomainEvents()
    {
        throw new NotImplementedException();
    }
}
