namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.ReferenceData.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Icon).HasMaxLength(100);
        builder.Property(c => c.IsSystem).HasDefaultValue(true);
    }
}
