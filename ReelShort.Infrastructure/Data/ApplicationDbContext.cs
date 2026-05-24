using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ReelShort.Domain.Entities;

namespace ReelShort.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users => Set<User>();
    public DbSet<Video> Videos => Set<Video>();
    public DbSet<Music> Musics => Set<Music>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<Hashtag> Hashtags => Set<Hashtag>();
    public DbSet<VideoHashtag> VideoHashtags => Set<VideoHashtag>();
    public DbSet<CommentLike> CommentLikes => Set<CommentLike>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // OnModelCreating sẽ được gọi khi EF Core xây dựng mô hình dữ liệu.
    // Đây là nơi bạn có thể cấu hình các mối quan hệ, ràng buộc, và các thiết lập khác cho các entity của bạn.
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // EF Core sẽ quét toàn bộ assembly hiện tại để tìm và áp dụng tất cả các cấu hình entity (các lớp triển khai IEntityTypeConfiguration<T>)
        // tự động apply các configuration đó vào model
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
    
    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=ReelShort;Username=ducdev97;Password=Admin@123");
    // }
}