using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReelShort.Domain.Entities;
using ReelShort.Domain.Enums;
using ReelShort.Infrastructure.Data.Constants;

namespace ReelShort.Infrastructure.Data.Configurations;

public class VideoConfiguration : IEntityTypeConfiguration<Video>
{
    public void Configure(EntityTypeBuilder<Video> builder)
    {
        builder.ToTable(TableNames.Videos);
        
        builder.HasIndex(v => v.CreatedAt);

        builder.Property(v => v.VideoUrl).IsRequired();
        builder.Property(v => v.Caption).HasMaxLength(ColumnLengths.MaxCaptionLength);
        builder.Property(v => v.Status).HasDefaultValue(VideoStatus.Draft);

        builder.Property(v => v.ViewCount).HasDefaultValue(0);
        builder.Property(v => v.LikeCount).HasDefaultValue(0);
        builder.Property(v => v.CommentCount).HasDefaultValue(0);
        builder.Property(v => v.ShareCount).HasDefaultValue(0);

        builder.HasOne(v => v.User)
            .WithMany(u => u.Videos)
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.Music)
            .WithMany(m => m.Videos)
            .HasForeignKey(v => v.MusicId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}