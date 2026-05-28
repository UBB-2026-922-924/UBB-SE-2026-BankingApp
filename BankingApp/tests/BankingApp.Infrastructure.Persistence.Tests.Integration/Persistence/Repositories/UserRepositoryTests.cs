namespace BankingApp.Infrastructure.Persistence.Tests.Integration.Persistence.Repositories;

using TestSupport;

[Collection(IntegrationTestCollectionNames.Name)]
public sealed class UserRepositoryTests(SqlServerDatabaseFixture databaseFixture)
    : IntegrationTestBase(databaseFixture)
{
    [Fact(Skip = "Not implemented yet.")]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnUser()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task GetByEmailAsync_WhenEmailExists_ShouldReturnUser()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task GetByEmailAsync_WhenEmailDoesNotExist_ShouldReturnNull()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task AddAsync_WhenUserIsValid_ShouldPersistUser()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task UpdateAsync_WhenUserExists_ShouldPersistChanges()
    {
        throw new NotImplementedException();
    }
}
