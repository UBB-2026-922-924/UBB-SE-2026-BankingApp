namespace BankingApp.Infrastructure.Persistence.Tests.Integration.Persistence.Repositories;

using TestSupport;

[Collection(IntegrationTestCollectionNames.Name)]
public sealed class ForexRepositoryTests(SqlServerDatabaseFixture databaseFixture)
    : IntegrationTestBase(databaseFixture)
{
    [Fact(Skip = "Not implemented yet.")]
    public async Task GetByIdAsync_WhenForexTransactionExists_ShouldReturnForexTransaction()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task GetByIdAsync_WhenForexTransactionDoesNotExist_ShouldReturnNull()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task ListByUserIdAsync_WhenUserHasForexTransactions_ShouldReturnForexTransactions()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task ListByUserIdAsync_WhenUserHasNoForexTransactions_ShouldReturnEmptyCollection()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task AddAsync_WhenForexTransactionIsValid_ShouldPersistForexTransaction()
    {
        throw new NotImplementedException();
    }
}
