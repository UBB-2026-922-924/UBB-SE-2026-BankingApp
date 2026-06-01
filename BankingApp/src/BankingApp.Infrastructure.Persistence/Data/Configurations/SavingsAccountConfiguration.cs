namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.Aggregates.SavingsAggregate;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class SavingsAccountConfiguration : IEntityTypeConfiguration<SavingsAccount>
{
    public void Configure(EntityTypeBuilder<SavingsAccount> builder)
    {
        builder.ToTable("SavingsAccount");
        builder.HasKey(a => a.Id);
        builder.Ignore(a => a.DomainEvents);

        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.SavingsType).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(a => a.Balance).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(a => a.AccruedInterest).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(a => a.AnnualPercentageYield).HasColumnType("decimal(6,4)").IsRequired();
        builder.Property(a => a.AccountStatus).HasMaxLength(20).IsRequired();
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt);
        builder.Property(a => a.AccountName).HasMaxLength(100);
        builder.Property(a => a.FundingAccountId);
        builder.Property(a => a.TargetAmount).HasColumnType("decimal(18,2)");
        builder.Property(a => a.TargetDate);
        builder.Property(a => a.MaturityDate);

        builder.Ignore(a => a.MonthlyInterestProjection);
        builder.Ignore(a => a.ProgressPercent);
        builder.Ignore(a => a.FormattedBalance);
        builder.Ignore(a => a.IsGoalSavings);
        builder.Ignore(a => a.DisplayStatus);

        builder.HasIndex(a => a.UserId);
    }
}
