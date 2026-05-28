namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.Aggregates.TransferAggregate;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.ToTable("Transfers");
        builder.HasKey(transfer => transfer.Id);
        builder.Ignore(transfer => transfer.DomainEvents);

        builder.Property(transfer => transfer.UserId).IsRequired();
        builder.Property(transfer => transfer.SourceAccountId).IsRequired();
        builder.Property(transfer => transfer.LedgerTransactionId);
        builder.Property(transfer => transfer.RecipientName).HasMaxLength(200).IsRequired();
        builder.Property(transfer => transfer.RecipientIban)
            .HasConversion(PersistenceValueConverters.IbanConverter)
            .HasMaxLength(34)
            .IsRequired();
        builder.Property(transfer => transfer.RecipientBankName).HasMaxLength(200);
        builder.Property(transfer => transfer.Amount)
            .HasConversion(PersistenceValueConverters.MoneyConverter)
            .Metadata.SetValueComparer(PersistenceValueConverters.MoneyComparer);
        builder.Property(transfer => transfer.Amount).HasMaxLength(64).IsRequired();
        builder.Property(transfer => transfer.ConvertedAmount)
            .HasConversion(PersistenceValueConverters.NullableMoneyConverter)
            .Metadata.SetValueComparer(PersistenceValueConverters.NullableMoneyComparer);
        builder.Property(transfer => transfer.ConvertedAmount).HasMaxLength(64);
        builder.Property(transfer => transfer.ExchangeRate).HasColumnType("decimal(18,6)");
        builder.Property(transfer => transfer.Fee)
            .HasConversion(PersistenceValueConverters.MoneyConverter)
            .Metadata.SetValueComparer(PersistenceValueConverters.MoneyComparer);
        builder.Property(transfer => transfer.Fee).HasMaxLength(64).IsRequired();
        builder.Property(transfer => transfer.Reference).HasMaxLength(200);
        builder.Property(transfer => transfer.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .HasDefaultValue(TransferStatus.Pending)
            .IsRequired();
        builder.Property(transfer => transfer.EstimatedArrival);
        builder.Property(transfer => transfer.CreatedAt).IsRequired();
    }
}
