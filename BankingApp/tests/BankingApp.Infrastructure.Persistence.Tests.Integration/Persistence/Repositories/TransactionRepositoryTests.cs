namespace BankingApp.Infrastructure.Persistence.Tests.Integration.Persistence.Repositories;

using TestSupport;

[Collection(IntegrationTestCollectionNames.Name)]
public sealed class TransactionRepositoryTests(SqlServerDatabaseFixture databaseFixture)
    : IntegrationTestBase(databaseFixture)
{
    [Fact(Skip = "Not implemented yet.")]
    public async Task GetByIdAsync_WhenTransactionExists_ShouldReturnTransaction()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task GetByIdAsync_WhenTransactionDoesNotExist_ShouldReturnNull()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task ListByAccountIdAsync_WhenAccountHasTransactions_ShouldReturnTransactions()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task ListByAccountIdAsync_WhenAccountHasNoTransactions_ShouldReturnEmptyCollection()
    {
        throw new NotImplementedException();
    }
}
