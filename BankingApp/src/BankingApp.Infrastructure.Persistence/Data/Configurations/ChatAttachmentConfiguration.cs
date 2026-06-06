namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.Aggregates.ChatAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class ChatAttachmentConfiguration : IEntityTypeConfiguration<ChatAttachment>
{
    public void Configure(EntityTypeBuilder<ChatAttachment> builder)
    {
        builder.ToTable("ChatAttachment");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.ChatMessageId).HasColumnName("messageId").IsRequired();
        builder.Property(a => a.FileName).HasColumnName("attachmentName").HasMaxLength(255).IsRequired();
        builder.Property(a => a.ContentType).HasColumnName("fileType").HasMaxLength(50).IsRequired();
        builder.Property(a => a.FileSizeBytes).HasColumnName("fileSizeBytes").IsRequired();
        builder.Property(a => a.StoragePath).HasColumnName("storageUrl").HasMaxLength(255).IsRequired();

        builder.HasOne<ChatMessage>()
            .WithMany(m => m.Attachments)
            .HasForeignKey(a => a.ChatMessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
