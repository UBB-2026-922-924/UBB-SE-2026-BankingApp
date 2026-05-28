namespace BankingApp.Infrastructure.Persistence.Data.Seeders;

using Domain.Enums;
using Domain.ReferenceData.Billers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
///     Seeds the <see cref="Biller"/> reference table with a representative set of billers.
///     The seeder is idempotent: it only inserts billers whose name does not yet exist.
/// </summary>
internal static partial class BillerSeeder
{
    internal static async Task SeedAsync(AppDbContext dbContext, ILogger logger, CancellationToken cancellationToken = default)
    {
        List<Biller> defaults = GetDefaultBillers();

        var existingNames = (await dbContext.Billers
            .Select(biller => biller.Name)
            .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var toInsert = defaults
            .Where(biller => !existingNames.Contains(biller.Name))
            .ToList();

        if (toInsert.Count == 0)
        {
            LogAllPresent(logger, defaults.Count);
            return;
        }

        dbContext.Billers.AddRange(toInsert);
        await dbContext.SaveChangesAsync(cancellationToken);
        LogInserted(logger, toInsert.Count);
    }

    private static List<Biller> GetDefaultBillers() =>
    [
        // Utilities
        new() { Name = "Electrica SA",                 Category = BillerCategory.Utilities },
        new() { Name = "E.ON Energie Romania",         Category = BillerCategory.Utilities },
        new() { Name = "CEZ Distributie",              Category = BillerCategory.Utilities },
        new() { Name = "Distrigaz Sud",                Category = BillerCategory.Utilities },
        new() { Name = "Apa Nova Bucuresti",           Category = BillerCategory.Utilities },
        new() { Name = "RADET Bucuresti",              Category = BillerCategory.Utilities },

        // Telecom
        new() { Name = "Orange Romania",               Category = BillerCategory.Telecom },
        new() { Name = "Vodafone Romania",             Category = BillerCategory.Telecom },
        new() { Name = "Digi Communications",          Category = BillerCategory.Telecom },
        new() { Name = "Telekom Romania",              Category = BillerCategory.Telecom },
        new() { Name = "UPC Romania",                  Category = BillerCategory.Telecom },

        // Insurance
        new() { Name = "Allianz-Tiriac Asigurari",    Category = BillerCategory.Insurance },
        new() { Name = "Groupama Asigurari",           Category = BillerCategory.Insurance },
        new() { Name = "Omniasig Vienna Insurance",   Category = BillerCategory.Insurance },
        new() { Name = "ASIROM VIG",                   Category = BillerCategory.Insurance },
        new() { Name = "Generali Romania",             Category = BillerCategory.Insurance },

        // Government
        new() { Name = "ANAF Taxe si Impozite",        Category = BillerCategory.Government },
        new() { Name = "Primaria Generala Bucuresti",  Category = BillerCategory.Government },
        new() { Name = "Directia Taxe Locale",         Category = BillerCategory.Government },
        new() { Name = "RAR Registrul Auto Roman",     Category = BillerCategory.Government },

        // Rent
        new() { Name = "Imobiliare.ro Chirie",         Category = BillerCategory.Rent },
        new() { Name = "Colliers Property Management", Category = BillerCategory.Rent },

        // Subscriptions
        new() { Name = "Netflix Romania",              Category = BillerCategory.Subscriptions },
        new() { Name = "Spotify",                      Category = BillerCategory.Subscriptions },
        new() { Name = "Microsoft 365",                Category = BillerCategory.Subscriptions },
        new() { Name = "Adobe Creative Cloud",         Category = BillerCategory.Subscriptions },
        new() { Name = "HBO Max Romania",              Category = BillerCategory.Subscriptions },
    ];

    [LoggerMessage(EventId = 5000, Level = LogLevel.Information,
        Message = "BillerSeeder: all {Count} default billers already present, skipping.")]
    private static partial void LogAllPresent(ILogger logger, int count);

    [LoggerMessage(EventId = 5001, Level = LogLevel.Information,
        Message = "BillerSeeder: inserted {Count} new billers.")]
    private static partial void LogInserted(ILogger logger, int count);
}
