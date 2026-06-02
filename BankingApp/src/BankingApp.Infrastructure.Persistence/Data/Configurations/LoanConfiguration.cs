namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.Aggregates.LoanAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("Loan");
        builder.HasKey(l => l.Id);
        builder.Ignore(l => l.DomainEvents);

        builder.Property(l => l.UserId).IsRequired();
        builder.Property(l => l.LoanType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(l => l.Principal).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(l => l.OutstandingBalance).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(l => l.InterestRate).HasColumnType("decimal(6,4)").IsRequired();
        builder.Property(l => l.MonthlyInstallment).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(l => l.RemainingMonths).IsRequired();
        builder.Property(l => l.LoanStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(l => l.TermInMonths).IsRequired();
        builder.Property(l => l.StartDate).IsRequired();

        builder.HasIndex(l => l.UserId);
    }
}
