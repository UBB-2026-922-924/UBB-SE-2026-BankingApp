namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.Aggregates.ChatAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessage");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.ChatSessionId).IsRequired();
        builder.Property(m => m.Sender).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(m => m.Content).HasMaxLength(4000).IsRequired();
        builder.Property(m => m.SentAt).IsRequired();

        builder.Ignore(m => m.Attachments);

        builder.HasIndex(m => m.ChatSessionId);
    }
}
