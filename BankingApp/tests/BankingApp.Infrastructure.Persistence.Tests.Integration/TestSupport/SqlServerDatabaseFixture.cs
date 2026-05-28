namespace BankingApp.Infrastructure.Persistence.Tests.Integration.TestSupport;

using Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Respawn;
using Testcontainers.MsSql;

/// <summary>
///     Provides a repeatable SQL Server database via Testcontainers for integration tests.
///     One SQL Server instance is shared across the xUnit integration-test collection.
///     Call <see cref="ResetAsync" /> before each test to wipe all data cleanly.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global - xUnit instantiates fixtures via reflection.
public sealed class SqlServerDatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer? _databaseContainer;
    private readonly string? _externalConnectionString =
        Environment.GetEnvironmentVariable("MSSQL_CONNECTION_STRING");

    private string _connectionString = string.Empty;
    private SqlConnection? _connection;
    private Respawner? _respawner;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SqlServerDatabaseFixture" /> class.
    /// </summary>
    public SqlServerDatabaseFixture()
    {
        if (_externalConnectionString is null)
        {
            _databaseContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04")
                .Build();
        }
    }

    /// <summary>
    ///     Gets the active connection string used by the integration database.
    /// </summary>
    public string ConnectionString => _connectionString;

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        if (_databaseContainer is not null)
        {
            await _databaseContainer.StartAsync();
            _connectionString = _databaseContainer.GetConnectionString();
        }
        else
        {
            _connectionString = _externalConnectionString!;
        }

        await using AppDbContext dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();

        _connection = new SqlConnection(_connectionString);
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(
            _connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer,
                SchemasToInclude = ["dbo"]
            });
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }

        if (_databaseContainer is not null)
        {
            await _databaseContainer.DisposeAsync();
        }
    }

    /// <summary>
    ///     Creates a fresh <see cref="AppDbContext" /> connected to the integration SQL Server instance.
    /// </summary>
    /// <returns>A new <see cref="AppDbContext" />.</returns>
    public AppDbContext CreateDbContext()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(_connectionString)
            .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;

        return new AppDbContext(options);
    }

    /// <summary>
    ///     Wipes all data from the database using Respawn.
    /// </summary>
    public async Task ResetAsync()
    {
        if (_respawner != null && _connection != null)
        {
            await _respawner.ResetAsync(_connection);
        }
    }
}
