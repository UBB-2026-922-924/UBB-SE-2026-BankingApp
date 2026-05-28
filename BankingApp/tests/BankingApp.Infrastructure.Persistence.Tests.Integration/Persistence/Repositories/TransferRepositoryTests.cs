namespace BankingApp.Infrastructure.Persistence.Tests.Integration.Persistence.Repositories;

using TestSupport;

[Collection(IntegrationTestCollectionNames.Name)]
public sealed class TransferRepositoryTests(SqlServerDatabaseFixture databaseFixture)
    : IntegrationTestBase(databaseFixture)
{
    [Fact(Skip = "Not implemented yet.")]
    public async Task GetByIdAsync_WhenTransferExists_ShouldReturnTransfer()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task GetByIdAsync_WhenTransferDoesNotExist_ShouldReturnNull()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task ListByUserIdAsync_WhenUserHasTransfers_ShouldReturnTransfers()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task ListByUserIdAsync_WhenUserHasNoTransfers_ShouldReturnEmptyCollection()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task AddAsync_WhenTransferIsValid_ShouldPersistTransfer()
    {
        throw new NotImplementedException();
    }
}
