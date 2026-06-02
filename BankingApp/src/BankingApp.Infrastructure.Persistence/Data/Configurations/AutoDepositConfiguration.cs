namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.Aggregates.SavingsAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class AutoDepositConfiguration : IEntityTypeConfiguration<AutoDeposit>
{
    public void Configure(EntityTypeBuilder<AutoDeposit> builder)
    {
        builder.ToTable("AutoDeposit");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.SavingsAccountId).IsRequired();
        builder.Property(d => d.Amount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(d => d.Frequency).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(d => d.NextRunDate).IsRequired();
        builder.Property(d => d.IsActive).IsRequired();
        builder.Property(d => d.SourceAccountId);
        builder.Property(d => d.DayOfMonth);
        builder.Property(d => d.DayOfWeek);
        builder.Property(d => d.UpdatedAt);

        builder.HasIndex(d => d.SavingsAccountId);
    }
}
