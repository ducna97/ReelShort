using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReelShort.Domain.Entities;
using ReelShort.Infrastructure.Data.Constants;

namespace ReelShort.Infrastructure.Data.Configurations;

public class MusicConfiguration : IEntityTypeConfiguration<Music>
{
    public void Configure(EntityTypeBuilder<Music> builder)
    {
        builder.ToTable(TableNames.Musics);
        
        builder.Property(m => m.Name).IsRequired().HasMaxLength(ColumnLengths.MaxNameLength);
        builder.Property(m => m.Author).HasMaxLength(150);
        builder.Property(m => m.AudioUrl).IsRequired();
        builder.Property(m => m.CoverImageUrl).IsRequired();
        
        builder.Property(m => m.UseCount).HasDefaultValue(0);
    }
}