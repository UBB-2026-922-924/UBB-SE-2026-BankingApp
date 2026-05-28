namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.ReferenceData.Billers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class BillerConfiguration : IEntityTypeConfiguration<Biller>
{
    public void Configure(EntityTypeBuilder<Biller> builder)
    {
        builder.ToTable("Billers");
        builder.HasKey(biller => biller.Id);
        builder.Property(biller => biller.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(biller => biller.Name).IsUnique();
        builder.Property(biller => biller.Category).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(biller => biller.LogoUrl).HasMaxLength(500);
        builder.Property(biller => biller.IsActive).HasDefaultValue(true);
    }
}
