using GRWMJobs.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GRWMJobs.DAL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Subcategory> Subcategories { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<Image> Images { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User - Question
            modelBuilder.Entity<User>()
                .HasMany(u => u.Questions)
                .WithOne(q => q.User)
                .HasForeignKey(q => q.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User - Answer
            modelBuilder.Entity<User>()
                .HasMany(u => u.Answers)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Question - Answer
            modelBuilder.Entity<Question>()
                .HasMany(q => q.Answers)
                .WithOne(a => a.Question)
                .HasForeignKey(a => a.QuestionId);

            // Track - Category
            modelBuilder.Entity<Track>()
                .HasMany(t => t.Categories)
                .WithOne(c => c.Track)
                .HasForeignKey(c => c.TrackId);

            // Track - Question
            modelBuilder.Entity<Track>()
                .HasMany(t => t.Questions)
                .WithOne(q => q.Track)
                .HasForeignKey(q => q.TrackId);

            // Category - Question
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Questions)
                .WithOne(q => q.Category)
                .HasForeignKey(q => q.CategoryId);

            // Category - Subcategory
            modelBuilder.Entity<Category>()
                .HasMany<Subcategory>()
                .WithOne(sc => sc.Category)
                .HasForeignKey(sc => sc.CategoryId);

            // Subcategory - Question (optional)
            modelBuilder.Entity<Subcategory>()
                .HasMany(sc => sc.Questions!)
                .WithOne(q => q.Subcategory)
                .HasForeignKey(q => q.SubcategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Simple constraints
            modelBuilder.Entity<Track>().Property(t => t.Name).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<Category>().Property(c => c.Name).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<Question>().Property(q => q.Title).HasMaxLength(150).IsRequired();

            // Session FK
            modelBuilder.Entity<UserSession>()
                .HasOne(us => us.User)
                .WithMany()
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Image relationships
            modelBuilder.Entity<Image>()
                .HasOne(i => i.Question)
                .WithMany(q => q.Images)
                .HasForeignKey(i => i.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Image>()
                .HasOne(i => i.Answer)
                .WithMany(a => a.Images)
                .HasForeignKey(i => i.AnswerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Image>()
                .HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Image constraints
            modelBuilder.Entity<Image>().Property(i => i.FileName).HasMaxLength(255).IsRequired();
            modelBuilder.Entity<Image>().Property(i => i.FilePath).HasMaxLength(500).IsRequired();
        }
    }
}
