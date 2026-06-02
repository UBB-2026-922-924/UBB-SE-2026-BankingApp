namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.Aggregates.LoanAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class AmortizationRowConfiguration : IEntityTypeConfiguration<AmortizationRow>
{
    public void Configure(EntityTypeBuilder<AmortizationRow> builder)
    {
        builder.ToTable("AmortizationRow");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.LoanId).IsRequired();
        builder.Property(r => r.InstallmentNumber).IsRequired();
        builder.Property(r => r.DueDate).IsRequired();
        builder.Property(r => r.PrincipalPortion).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(r => r.InterestPortion).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(r => r.RemainingBalance).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(r => r.IsCurrent).IsRequired();

        builder.HasIndex(r => r.LoanId);
    }
}
