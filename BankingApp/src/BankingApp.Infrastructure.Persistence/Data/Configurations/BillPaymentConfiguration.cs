namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.Aggregates.BillPaymentAggregate;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class BillPaymentConfiguration : IEntityTypeConfiguration<BillPayment>
{
    public void Configure(EntityTypeBuilder<BillPayment> builder)
    {
        builder.ToTable("BillPayments");
        builder.HasKey(billPayment => billPayment.Id);
        builder.Ignore(billPayment => billPayment.DomainEvents);

        builder.Property(billPayment => billPayment.UserId).IsRequired();
        builder.Property(billPayment => billPayment.SourceAccountId).IsRequired();
        builder.Property(billPayment => billPayment.BillerId).IsRequired();
        builder.Property(billPayment => billPayment.LedgerTransactionId);
        builder.Property(billPayment => billPayment.BillerReference).HasMaxLength(200).IsRequired();
        builder.Property(billPayment => billPayment.Amount)
            .HasConversion(PersistenceValueConverters.MoneyConverter)
            .Metadata.SetValueComparer(PersistenceValueConverters.MoneyComparer);
        builder.Property(billPayment => billPayment.Amount).HasMaxLength(64).IsRequired();
        builder.Property(billPayment => billPayment.Fee)
            .HasConversion(PersistenceValueConverters.MoneyConverter)
            .Metadata.SetValueComparer(PersistenceValueConverters.MoneyComparer);
        builder.Property(billPayment => billPayment.Fee).HasMaxLength(64).IsRequired();
        builder.Property(billPayment => billPayment.ReceiptNumber).HasMaxLength(100);
        builder.Property(billPayment => billPayment.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .HasDefaultValue(BillPaymentStatus.Pending)
            .IsRequired();
        builder.Property(billPayment => billPayment.CreatedAt).IsRequired();
    }
}
