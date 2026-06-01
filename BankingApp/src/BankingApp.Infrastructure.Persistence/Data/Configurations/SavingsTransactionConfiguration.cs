namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.Aggregates.SavingsAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class SavingsTransactionConfiguration : IEntityTypeConfiguration<SavingsTransaction>
{
    public void Configure(EntityTypeBuilder<SavingsTransaction> builder)
    {
        builder.ToTable("SavingsTransaction");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.SavingsAccountId).IsRequired();
        builder.Property(t => t.Amount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(t => t.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(t => t.Source).HasMaxLength(100);
        builder.Property(t => t.AccountId).IsRequired();
        builder.Property(t => t.BalanceAfter).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(500);

        builder.HasIndex(t => t.SavingsAccountId);
    }
}
