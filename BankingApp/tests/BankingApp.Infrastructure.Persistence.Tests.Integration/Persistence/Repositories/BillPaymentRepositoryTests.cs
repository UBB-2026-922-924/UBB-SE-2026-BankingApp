namespace BankingApp.Infrastructure.Persistence.Tests.Integration.Persistence.Repositories;

using TestSupport;

[Collection(IntegrationTestCollectionNames.Name)]
public sealed class BillPaymentRepositoryTests(SqlServerDatabaseFixture databaseFixture)
    : IntegrationTestBase(databaseFixture)
{
    [Fact(Skip = "Not implemented yet.")]
    public async Task GetByIdAsync_WhenBillPaymentExists_ShouldReturnBillPayment()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task GetByIdAsync_WhenBillPaymentDoesNotExist_ShouldReturnNull()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task ListByUserIdAsync_WhenUserHasBillPayments_ShouldReturnBillPayments()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task ListByUserIdAsync_WhenUserHasNoBillPayments_ShouldReturnEmptyCollection()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public async Task AddAsync_WhenBillPaymentIsValid_ShouldPersistBillPayment()
    {
        throw new NotImplementedException();
    }
}
