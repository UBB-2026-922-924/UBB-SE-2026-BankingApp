namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.Aggregates.InvestmentAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class InvestmentHoldingConfiguration : IEntityTypeConfiguration<InvestmentHolding>
{
    public void Configure(EntityTypeBuilder<InvestmentHolding> builder)
    {
        builder.ToTable("InvestmentHolding");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.PortfolioId).IsRequired();
        builder.Property(h => h.Ticker).HasMaxLength(20).IsRequired();
        builder.Property(h => h.AssetType).HasMaxLength(50).IsRequired();
        builder.Property(h => h.Quantity).HasColumnType("decimal(18,8)").IsRequired();
        builder.Property(h => h.AvgPurchasePrice).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(h => h.CurrentPrice).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(h => h.UnrealizedGainLoss).HasColumnType("decimal(18,4)").IsRequired();

        builder.HasIndex(h => new { h.PortfolioId, h.Ticker }).IsUnique();
    }
}
