namespace BankingApp.Infrastructure.Persistence.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

/// <summary>
/// Design-time factory used exclusively by EF Core CLI tooling (<c>dotnet ef migrations add</c>,
/// <c>dotnet ef database update</c>, etc.). Never instantiated at runtime.
/// <para>
/// The connection string is read from the <c>ConnectionStrings__BankingAppDb</c> environment
/// variable, falling back to a local dev default if not set.
/// </para>
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        string connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__BankingAppDb")
            ?? "Server=localhost;Database=BankingApp;Trusted_Connection=True;TrustServerCertificate=True;";

        DbContextOptionsBuilder<AppDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
