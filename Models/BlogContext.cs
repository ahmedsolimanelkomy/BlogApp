namespace BlogApp.Models
{
    using Microsoft.EntityFrameworkCore;

    namespace BlogApp.Models
    {
        public class BlogContext : DbContext
        {
            public BlogContext(DbContextOptions<BlogContext> options) : base(options) { }
            public DbSet<User> Users { get; set; }
            public DbSet<Post> Posts { get; set; }
            public DbSet<Comment> Comments { get; set; }
            public DbSet<Tag> Tags { get; set; }
            public DbSet<PostTag> PostTags { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<PostTag>()
                    .HasKey(pt => new { pt.PostId, pt.TagId });
                modelBuilder.Entity<User>()
                    .HasIndex(u => u.Email)
                    .IsUnique();
            }
        }
    }
}
