using Microsoft.EntityFrameworkCore;
using Redliner.Models;
using System.IO;

namespace Redliner.Data;

/// <summary>
/// Entity Framework context for the Redliner application
/// </summary>
public class RedlinerDbContext : DbContext
{
    public DbSet<Document> Documents { get; set; }
    public DbSet<Annotation> Annotations { get; set; }
    public DbSet<UserPreference> UserPreferences { get; set; }

    public RedlinerDbContext()
    {
    }

    public RedlinerDbContext(DbContextOptions<RedlinerDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Default SQLite database path
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                                    "Redliner", "redliner.db");
            
            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Document entity
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FilePath).IsRequired();
            entity.Property(e => e.FileName).IsRequired();
            entity.HasIndex(e => e.FilePath).IsUnique();
        });

        // Configure Annotation entity
        modelBuilder.Entity<Annotation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired();
            entity.HasOne(e => e.Document)
                  .WithMany(d => d.Annotations)
                  .HasForeignKey(e => e.DocumentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure UserPreference entity
        modelBuilder.Entity<UserPreference>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired();
            entity.HasIndex(e => e.Key).IsUnique();
        });
    }
}