namespace BankingApp.Infrastructure.Persistence.Tests.Integration.TestSupport;

using Data;

/// <summary>
///     Central place for reusable integration-test seed helpers.
///     Add concrete builders and seed methods as tests are implemented.
/// </summary>
public sealed class TestDataSeeder(AppDbContext dbContext)
{
    public AppDbContext DbContext { get; } = dbContext;
}
