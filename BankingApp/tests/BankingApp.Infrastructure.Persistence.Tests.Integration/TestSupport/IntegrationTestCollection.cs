namespace BankingApp.Infrastructure.Persistence.Tests.Integration.TestSupport;

public static class IntegrationTestCollectionNames
{
    public const string Name = "Infrastructure Integration";
}

[CollectionDefinition(IntegrationTestCollectionNames.Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<SqlServerDatabaseFixture>;
