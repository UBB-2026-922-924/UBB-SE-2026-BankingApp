namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.Aggregates.SavedBillerAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class SavedBillerConfiguration : IEntityTypeConfiguration<SavedBiller>
{
    public void Configure(EntityTypeBuilder<SavedBiller> builder)
    {
        builder.ToTable("SavedBillers");
        builder.HasKey(savedBiller => savedBiller.Id);
        builder.Ignore(savedBiller => savedBiller.DomainEvents);

        builder.Property(savedBiller => savedBiller.UserId).IsRequired();
        builder.Property(savedBiller => savedBiller.BillerId).IsRequired();
        builder.Property(savedBiller => savedBiller.Nickname).HasMaxLength(200);
        builder.Property(savedBiller => savedBiller.DefaultReference).HasMaxLength(200);
        builder.Property(savedBiller => savedBiller.CreatedAt).IsRequired();

        builder.HasIndex(savedBiller => new { savedBiller.UserId, savedBiller.BillerId }).IsUnique();
    }
}
