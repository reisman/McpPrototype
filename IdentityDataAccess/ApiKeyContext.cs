using Microsoft.EntityFrameworkCore;

namespace IdentityDataAccess;

internal sealed class ApiKeyContext(DbContextOptions<ApiKeyContext> options) : DbContext(options)
{
    private const string DatabaseName = "IdentityDB";
    
    internal static ApiKeyContext Create()
    {
        var options = new DbContextOptionsBuilder<ApiKeyContext>()
            .UseSqlite($"DataSource={DatabaseName}")
            .Options;
        
        var context = new ApiKeyContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        
        return context;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        var apiKeyEntity = modelBuilder
            .Entity<ApiKey>()
            .ToTable("ApiKeys");
            
        apiKeyEntity
            .HasKey(p => p.Id)
            .HasAnnotation("Sqlite:Autoincrement", true);

        apiKeyEntity
            .Property(p => p.Key)
            .HasMaxLength(255)
            .IsRequired();

        apiKeyEntity
            .HasIndex(p => p.Key)
            .IsUnique();
    }

    internal DbSet<ApiKey> ApiKeys { get; set; } = null!;
}