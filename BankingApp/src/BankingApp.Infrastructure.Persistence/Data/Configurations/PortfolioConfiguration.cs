namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.Aggregates.InvestmentAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
{
    public void Configure(EntityTypeBuilder<Portfolio> builder)
    {
        builder.ToTable("Portfolio");
        builder.HasKey(p => p.Id);
        builder.Ignore(p => p.DomainEvents);

        builder.Property(p => p.UserId).IsRequired();

        builder.Ignore(p => p.TotalValue);
        builder.Ignore(p => p.TotalCostBasis);
        builder.Ignore(p => p.TotalGainLoss);
        builder.Ignore(p => p.GainLossPercent);

        builder.HasIndex(p => p.UserId).IsUnique();
    }
}
