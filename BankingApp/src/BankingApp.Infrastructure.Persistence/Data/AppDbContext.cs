namespace BankingApp.Infrastructure.Persistence.Data;

using Domain.Aggregates.AccountAggregate;
using Domain.Aggregates.BeneficiaryAggregate;
using Domain.Aggregates.BillPaymentAggregate;
using Domain.Aggregates.ChatAggregate;
using Domain.Aggregates.ChatAggregate.Entities;
using Domain.Aggregates.ForexAggregate;
using Domain.Aggregates.IdentityAggregate;
using Domain.Aggregates.InvestmentAggregate;
using Domain.Aggregates.InvestmentAggregate.Entities;
using Domain.Aggregates.LoanAggregate;
using Domain.Aggregates.LoanAggregate.Entities;
using Domain.Aggregates.SavedBillerAggregate;
using Domain.Aggregates.SavingsAggregate;
using Domain.Aggregates.SavingsAggregate.Entities;
using Domain.Aggregates.TransferAggregate;
using Domain.Aggregates.UserAggregate;
using Domain.ReferenceData.Billers;
using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();

    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    public DbSet<User> Users => Set<User>();

    public DbSet<IdentityAccount> IdentityAccounts => Set<IdentityAccount>();

    public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();

    public DbSet<BillPayment> BillPayments => Set<BillPayment>();

    public DbSet<Biller> Billers => Set<Biller>();

    public DbSet<ForexTransaction> ForexTransactions => Set<ForexTransaction>();

    public DbSet<SavedBiller> SavedBillers => Set<SavedBiller>();

    public DbSet<Transfer> Transfers => Set<Transfer>();

    public DbSet<Portfolio> Portfolios => Set<Portfolio>();

    public DbSet<InvestmentHolding> InvestmentHoldings => Set<InvestmentHolding>();

    public DbSet<InvestmentTransaction> InvestmentTransactions => Set<InvestmentTransaction>();

    public DbSet<SavingsAccount> SavingsAccounts => Set<SavingsAccount>();

    public DbSet<SavingsTransaction> SavingsTransactions => Set<SavingsTransaction>();

    public DbSet<AutoDeposit> AutoDeposits => Set<AutoDeposit>();

    public DbSet<Loan> Loans => Set<Loan>();

    public DbSet<LoanApplication> LoanApplications => Set<LoanApplication>();

    public DbSet<AmortizationRow> AmortizationRows => Set<AmortizationRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
