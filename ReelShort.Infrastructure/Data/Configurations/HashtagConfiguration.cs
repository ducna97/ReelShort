using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReelShort.Domain.Entities;
using ReelShort.Infrastructure.Data.Constants;

namespace ReelShort.Infrastructure.Data.Configurations;

public class HashtagConfiguration : IEntityTypeConfiguration<Hashtag>
{
    public void Configure(EntityTypeBuilder<Hashtag> builder)
    {
        builder.ToTable(TableNames.Hashtags);
        
        // Unique Index so there will never be two duplicate tags
        builder.HasIndex(h => h.Name).IsUnique(); 

        builder.Property(h => h.Name).IsRequired().HasMaxLength(50);
        builder.Property(h => h.ViewCount).HasDefaultValue(0);
    }
}