namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.Aggregates.LoanAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class LoanApplicationConfiguration : IEntityTypeConfiguration<LoanApplication>
{
    public void Configure(EntityTypeBuilder<LoanApplication> builder)
    {
        builder.ToTable("LoanApplication");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.LoanType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.DesiredAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(a => a.PreferredTermMonths).IsRequired();
        builder.Property(a => a.Purpose).HasMaxLength(500).IsRequired();
        builder.Property(a => a.ApplicationStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.RejectionReason).HasMaxLength(500);

        builder.HasIndex(a => a.UserId);
    }
}
