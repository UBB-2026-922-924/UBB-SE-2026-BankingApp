namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.Aggregates.UserAggregate;
using Domain.Aggregates.UserAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class OAuthLinkConfiguration : IEntityTypeConfiguration<OAuthLink>
{
    public void Configure(EntityTypeBuilder<OAuthLink> builder)
    {
        builder.ToTable("OAuthLinks");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Provider).HasMaxLength(100).IsRequired();
        builder.Property(o => o.ProviderUserId).HasMaxLength(256).IsRequired();
        builder.Property(o => o.ProviderEmail).HasMaxLength(256);
        builder.Property(o => o.LinkedAt).IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(o => new { o.Provider, o.ProviderUserId }).IsUnique();
    }
}
