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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}