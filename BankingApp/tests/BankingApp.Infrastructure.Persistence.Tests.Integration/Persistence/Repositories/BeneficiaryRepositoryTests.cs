namespace BankingApp.Infrastructure.Persistence.Tests.Integration.Persistence.Repositories;

using TestSupport;

[Collection(IntegrationTestCollectionNames.Name)]
public sealed class BeneficiaryRepositoryTests(SqlServerDatabaseFixture databaseFixture)
    : IntegrationTestBase(databaseFixture)
{
    [Fact(Skip = "Not implemented yet.")]
    public async Task GetByIdAsync_WhenBeneficiaryExists_ShouldReturnBeneficiary()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task GetByIdAsync_WhenBeneficiaryDoesNotExist_ShouldReturnNull()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task ListByUserIdAsync_WhenUserHasBeneficiaries_ShouldReturnBeneficiaries()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task ListByUserIdAsync_WhenUserHasNoBeneficiaries_ShouldReturnEmptyCollection()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task AddAsync_WhenBeneficiaryIsValid_ShouldPersistBeneficiary()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task UpdateAsync_WhenBeneficiaryExists_ShouldPersistChanges()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task DeleteAsync_WhenBeneficiaryExists_ShouldRemoveBeneficiary()
    {
        throw new NotImplementedException();
    }
}
