using Microsoft.EntityFrameworkCore;

namespace BomDataAccess;

internal sealed class BomDbContext(DbContextOptions<BomDbContext> options) : DbContext(options)
{
    private const string DatabaseName = "BomDB";
    
    internal static BomDbContext Create()
    {
        var options = new DbContextOptionsBuilder<BomDbContext>()
            .UseSqlite($"DataSource={DatabaseName}")
            .Options;
        
        var context = new BomDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        
        return context;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        var partEntity = modelBuilder
            .Entity<Part>()
            .ToTable("Parts");
            
        partEntity
            .HasKey(p => p.Id)
            .HasAnnotation("Sqlite:Autoincrement", true);

        partEntity
            .Property(p => p.Name)
            .HasMaxLength(255)
            .IsRequired();
        
        partEntity
            .Property(p => p.Number)
            .HasMaxLength(255)
            .IsRequired();

        partEntity
            .HasOne(p => p.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey("ParentId")
            .HasPrincipalKey(nameof(Part.Id))
            .OnDelete(DeleteBehavior.Cascade);
    }

    internal DbSet<Part> Parts { get; set; } = null!;
}