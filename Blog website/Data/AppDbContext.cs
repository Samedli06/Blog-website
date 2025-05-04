using Microsoft.EntityFrameworkCore;
using Blog_website.Models;

namespace Blog_website.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
        // DbSet properties for each model
        public DbSet<Post> Posts { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<PostTag> PostTags { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships between models
            
            // Post to Author relationship (one-to-many)
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Author)
                .WithMany(a => a.Posts)
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // Post to Category relationship (one-to-many)
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.CategoryId)
                .IsRequired(false) // Category is optional
                .OnDelete(DeleteBehavior.SetNull); // Set null if category is deleted

            // Post to Comment relationship (one-to-many)
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade); // Delete comments when post is deleted

            // Comment to Parent Comment relationship (self-referencing)
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany() // No navigation property for child comments
                .HasForeignKey(c => c.ParentCommentId)
                .IsRequired(false) // Parent comment is optional
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // Post to Tag relationship (many-to-many through PostTag)
            modelBuilder.Entity<PostTag>()
                .HasOne(pt => pt.Post)
                .WithMany(p => p.PostTags)
                .HasForeignKey(pt => pt.PostId);

            modelBuilder.Entity<PostTag>()
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.PostTags)
                .HasForeignKey(pt => pt.TagId);

            // User to Author relationship (one-to-one)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Author)
                .WithOne() // No navigation property from Author to User
                .HasForeignKey<Author>("UserId") // Shadow property
                .IsRequired(false); // Author is optional for a User

            // Add unique constraints
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Slug)
                .IsUnique();

            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.Slug)
                .IsUnique();

            modelBuilder.Entity<Post>()
                .HasIndex(p => p.Slug)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Subscription>()
                .HasIndex(s => s.Email)
                .IsUnique();

            // User to Role relationship (many-to-many through UserRole)
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);
        }
    }
}
