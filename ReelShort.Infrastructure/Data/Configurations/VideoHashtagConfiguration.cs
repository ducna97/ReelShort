using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReelShort.Domain.Entities;
using ReelShort.Infrastructure.Data.Constants;

namespace ReelShort.Infrastructure.Data.Configurations;

public class VideoHashtagConfiguration : IEntityTypeConfiguration<VideoHashtag>
{
    public void Configure(EntityTypeBuilder<VideoHashtag> builder)
    {
        builder.ToTable(TableNames.VideoHashtags);
        
        builder.HasKey(vh => new { vh.VideoId, vh.HashtagId });

        builder.HasOne(vh => vh.Video)
            .WithMany(v => v.VideoHashtags)
            .HasForeignKey(vh => vh.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(vh => vh.Hashtag)
            .WithMany(h => h.VideoHashtags)
            .HasForeignKey(vh => vh.HashtagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}