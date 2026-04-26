using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReelShort.Domain.Entities;
using ReelShort.Infrastructure.Data.Constants;

namespace ReelShort.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(TableNames.Users);
        
        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
        
        builder.Property(u => u.Username).IsRequired().HasMaxLength(ColumnLengths.MaxUsernameLength);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(ColumnLengths.MaxEmailLength);
        builder.Property(u => u.DisplayName).HasMaxLength(ColumnLengths.MaxNameLength);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.Bio).HasMaxLength(500);
        
        builder.Property(u => u.FollowerCount).HasDefaultValue(0);
        builder.Property(u => u.FollowingCount).HasDefaultValue(0);
        builder.Property(u => u.TotalLikes).HasDefaultValue(0);
    }
}