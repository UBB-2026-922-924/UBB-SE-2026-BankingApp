namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.Aggregates.IdentityAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class IdentityAccountConfiguration : IEntityTypeConfiguration<IdentityAccount>
{
    public void Configure(EntityTypeBuilder<IdentityAccount> builder)
    {
        builder.ToTable("IdentityAccounts");
        builder.HasKey(identityAccount => identityAccount.Id);
        builder.Ignore(identityAccount => identityAccount.DomainEvents);

        builder.Property(identityAccount => identityAccount.UserId).IsRequired();
        builder.HasIndex(identityAccount => identityAccount.UserId).IsUnique();
        builder.Property(identityAccount => identityAccount.PasswordHash)
            .HasConversion(PersistenceValueConverters.NullableHashedPasswordConverter)
            .HasMaxLength(512);
        builder.Property(identityAccount => identityAccount.IsLocked).HasDefaultValue(false);
        builder.Property(identityAccount => identityAccount.LockoutEnd);
        builder.Property(identityAccount => identityAccount.FailedLoginAttempts).HasDefaultValue(0);

        builder.Navigation(identityAccount => identityAccount.Sessions).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(identityAccount => identityAccount.Sessions, sessions =>
        {
            sessions.ToTable("Sessions");
            sessions.WithOwner().HasForeignKey(session => session.IdentityAccountId);
            sessions.HasKey(session => session.Id);
            sessions.Property(session => session.Id).ValueGeneratedOnAdd();
            sessions.Property(session => session.Token).HasMaxLength(512).IsRequired();
            sessions.HasIndex(session => session.Token).IsUnique();
            sessions.Property(session => session.DeviceInfo).HasMaxLength(256);
            sessions.Property(session => session.Browser).HasMaxLength(128);
            sessions.Property(session => session.IpAddress).HasMaxLength(64);
            sessions.Property(session => session.LastActiveAt);
            sessions.Property(session => session.ExpiresAt).IsRequired();
            sessions.Property(session => session.IsRevoked).HasDefaultValue(false);
            sessions.Property(session => session.CreatedAt).IsRequired();
        });
    }
}
