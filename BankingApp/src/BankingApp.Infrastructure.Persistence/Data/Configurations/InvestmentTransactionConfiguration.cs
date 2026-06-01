namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.Aggregates.InvestmentAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class InvestmentTransactionConfiguration : IEntityTypeConfiguration<InvestmentTransaction>
{
    public void Configure(EntityTypeBuilder<InvestmentTransaction> builder)
    {
        builder.ToTable("InvestmentTransaction");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.HoldingId).IsRequired();
        builder.Property(t => t.Ticker).HasMaxLength(20).IsRequired();
        builder.Property(t => t.ActionType).HasMaxLength(10).IsRequired();
        builder.Property(t => t.Quantity).HasColumnType("decimal(18,8)").IsRequired();
        builder.Property(t => t.PricePerUnit).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(t => t.Fees).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(t => t.OrderType).HasMaxLength(50).IsRequired();
        builder.Property(t => t.ExecutedAt).IsRequired();

        builder.HasIndex(t => t.HoldingId);
    }
}
