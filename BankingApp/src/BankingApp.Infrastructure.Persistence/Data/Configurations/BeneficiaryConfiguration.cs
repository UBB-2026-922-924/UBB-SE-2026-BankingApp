namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.Aggregates.BeneficiaryAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class BeneficiaryConfiguration : IEntityTypeConfiguration<Beneficiary>
{
    public void Configure(EntityTypeBuilder<Beneficiary> builder)
    {
        builder.ToTable("Beneficiaries");
        builder.HasKey(beneficiary => beneficiary.Id);
        builder.Ignore(beneficiary => beneficiary.DomainEvents);

        builder.Property(beneficiary => beneficiary.UserId).IsRequired();
        builder.Property(beneficiary => beneficiary.Name).HasMaxLength(200).IsRequired();
        builder.Property(beneficiary => beneficiary.Iban)
            .HasConversion(PersistenceValueConverters.IbanConverter)
            .HasMaxLength(34)
            .IsRequired();
        builder.Property(beneficiary => beneficiary.BankName).HasMaxLength(200);
        builder.Property(beneficiary => beneficiary.LastTransferDate);
        builder.Property(beneficiary => beneficiary.TotalAmountSent).HasColumnType("decimal(18,2)");
        builder.Property(beneficiary => beneficiary.TransferCount);
        builder.Property(beneficiary => beneficiary.CreatedAt).IsRequired();

        builder.HasIndex(beneficiary => new { beneficiary.UserId, beneficiary.Iban }).IsUnique();
    }
}
