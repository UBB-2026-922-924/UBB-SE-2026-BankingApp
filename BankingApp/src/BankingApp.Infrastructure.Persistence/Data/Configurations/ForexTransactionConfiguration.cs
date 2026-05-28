namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.Aggregates.ForexAggregate;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class ForexTransactionConfiguration : IEntityTypeConfiguration<ForexTransaction>
{
    public void Configure(EntityTypeBuilder<ForexTransaction> builder)
    {
        builder.ToTable("ForexTransactions");
        builder.HasKey(forexTransaction => forexTransaction.Id);
        builder.Ignore(forexTransaction => forexTransaction.DomainEvents);

        builder.Property(forexTransaction => forexTransaction.UserId).IsRequired();
        builder.Property(forexTransaction => forexTransaction.SourceAccountId).IsRequired();
        builder.Property(forexTransaction => forexTransaction.TargetAccountId).IsRequired();
        builder.Property(forexTransaction => forexTransaction.SourceLedgerTransactionId);
        builder.Property(forexTransaction => forexTransaction.TargetLedgerTransactionId);
        builder.Property(forexTransaction => forexTransaction.SourceAmount)
            .HasConversion(PersistenceValueConverters.MoneyConverter)
            .Metadata.SetValueComparer(PersistenceValueConverters.MoneyComparer);
        builder.Property(forexTransaction => forexTransaction.SourceAmount).HasMaxLength(64).IsRequired();
        builder.Property(forexTransaction => forexTransaction.TargetAmount)
            .HasConversion(PersistenceValueConverters.MoneyConverter)
            .Metadata.SetValueComparer(PersistenceValueConverters.MoneyComparer);
        builder.Property(forexTransaction => forexTransaction.TargetAmount).HasMaxLength(64).IsRequired();
        builder.Property(forexTransaction => forexTransaction.ExchangeRate).HasColumnType("decimal(18,6)").IsRequired();
        builder.Property(forexTransaction => forexTransaction.Commission)
            .HasConversion(PersistenceValueConverters.MoneyConverter)
            .Metadata.SetValueComparer(PersistenceValueConverters.MoneyComparer);
        builder.Property(forexTransaction => forexTransaction.Commission).HasMaxLength(64).IsRequired();
        builder.Property(forexTransaction => forexTransaction.RateLockedAt);
        builder.Property(forexTransaction => forexTransaction.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .HasDefaultValue(ExchangeTransactionStatus.Pending)
            .IsRequired();
        builder.Property(forexTransaction => forexTransaction.CreatedAt).IsRequired();
    }
}
