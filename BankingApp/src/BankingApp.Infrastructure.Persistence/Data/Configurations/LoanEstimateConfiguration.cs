namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.Aggregates.LoanAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class LoanEstimateConfiguration : IEntityTypeConfiguration<LoanEstimate>
{
    public void Configure(EntityTypeBuilder<LoanEstimate> builder)
    {
        builder.ToTable("LoanEstimates");
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.IndicativeRate).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(e => e.MonthlyInstallment).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.TotalRepayable).HasColumnType("decimal(18,2)").IsRequired();
    }
}
