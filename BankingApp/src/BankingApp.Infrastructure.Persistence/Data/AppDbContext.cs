namespace BankingApp.Infrastructure.Persistence.Data;

using Domain.Aggregates.AccountAggregate;
using Domain.Aggregates.BeneficiaryAggregate;
using Domain.Aggregates.BillPaymentAggregate;
using Domain.Aggregates.ForexAggregate;
using Domain.Aggregates.IdentityAggregate;
using Domain.Aggregates.SavedBillerAggregate;
using Domain.Aggregates.TransferAggregate;
using Domain.Aggregates.UserAggregate;
using Domain.ReferenceData.Billers;
using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<User> Users => Set<User>();

    public DbSet<IdentityAccount> IdentityAccounts => Set<IdentityAccount>();

    public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();

    public DbSet<BillPayment> BillPayments => Set<BillPayment>();

    public DbSet<Biller> Billers => Set<Biller>();

    public DbSet<ForexTransaction> ForexTransactions => Set<ForexTransaction>();

    public DbSet<SavedBiller> SavedBillers => Set<SavedBiller>();

    public DbSet<Transfer> Transfers => Set<Transfer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
