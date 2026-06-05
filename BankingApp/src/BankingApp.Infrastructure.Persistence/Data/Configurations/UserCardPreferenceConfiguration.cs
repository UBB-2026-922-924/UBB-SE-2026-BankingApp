namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.Aggregates.UserAggregate;
using Domain.Aggregates.UserAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class UserCardPreferenceConfiguration : IEntityTypeConfiguration<UserCardPreference>
{
    public void Configure(EntityTypeBuilder<UserCardPreference> builder)
    {
        builder.ToTable("UserCardPreferences");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).ValueGeneratedNever(); // UserId is the primary key
        builder.Property(p => p.SortOption).HasMaxLength(100).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<UserCardPreference>(p => p.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
