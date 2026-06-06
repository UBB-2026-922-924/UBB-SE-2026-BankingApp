namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.Aggregates.AccountAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class TransactionCategoryOverrideConfiguration : IEntityTypeConfiguration<TransactionCategoryOverride>
{
    public void Configure(EntityTypeBuilder<TransactionCategoryOverride> builder)
    {
        builder.ToTable("TransactionCategoryOverrides");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.TransactionId).IsRequired();
        builder.Property(o => o.UserId).IsRequired();
        builder.Property(o => o.CategoryId).IsRequired();

        builder.HasIndex(o => o.TransactionId);
        builder.HasIndex(o => o.UserId);
        builder.HasIndex(o => o.CategoryId);
    }
}
