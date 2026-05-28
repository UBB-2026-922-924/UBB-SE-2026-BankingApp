namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.Aggregates.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(user => user.Id);
        builder.Ignore(user => user.DomainEvents);

        builder.Property(user => user.Email)
            .HasConversion(PersistenceValueConverters.EmailConverter)
            .HasMaxLength(255)
            .IsRequired();
        builder.HasIndex(user => user.Email).IsUnique();
        builder.Property(user => user.FullName).HasMaxLength(200).IsRequired();
        builder.Property(user => user.PhoneNumber).HasMaxLength(32);
        builder.Property(user => user.DateOfBirth);
        builder.Property(user => user.Address).HasMaxLength(256);
        builder.Property(user => user.Nationality).HasMaxLength(100);
        builder.Property(user => user.PreferredLanguage).HasMaxLength(16).IsRequired();
        builder.Property(user => user.CreatedAt).IsRequired();
        builder.Property(user => user.UpdatedAt).IsRequired();

        builder.Navigation(user => user.Notifications).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(user => user.NotificationPreferences).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(user => user.Notifications, notifications =>
        {
            notifications.ToTable("Notifications");
            notifications.WithOwner().HasForeignKey(notification => notification.UserId);
            notifications.HasKey(notification => notification.Id);
            notifications.Property(notification => notification.Id).ValueGeneratedOnAdd();
            notifications.Property(notification => notification.Title).HasMaxLength(200).IsRequired();
            notifications.Property(notification => notification.Message).HasMaxLength(2000).IsRequired();
            notifications.Property(notification => notification.Type).HasMaxLength(64).IsRequired();
            notifications.Property(notification => notification.Channel).HasMaxLength(64).IsRequired();
            notifications.Property(notification => notification.IsRead).HasDefaultValue(false);
            notifications.Property(notification => notification.RelatedEntityType).HasMaxLength(64);
            notifications.Property(notification => notification.RelatedEntityId);
            notifications.Property(notification => notification.CreatedAt).IsRequired();
        });

        builder.OwnsMany(user => user.NotificationPreferences, preferences =>
        {
            preferences.ToTable("NotificationPreferences");
            preferences.WithOwner().HasForeignKey(preference => preference.UserId);
            preferences.HasKey(preference => preference.Id);
            preferences.Property(preference => preference.Id).ValueGeneratedOnAdd();
            preferences.Property(preference => preference.Category).HasConversion<string>().HasMaxLength(64).IsRequired();
            preferences.Property(preference => preference.PushEnabled).HasDefaultValue(true);
            preferences.Property(preference => preference.EmailEnabled).HasDefaultValue(true);
            preferences.Property(preference => preference.SmsEnabled).HasDefaultValue(false);
            preferences.Property(preference => preference.MinAmountThreshold).HasColumnType("decimal(18,2)");
            preferences.HasIndex(preference => new { preference.UserId, preference.Category }).IsUnique();
        });
    }
}
