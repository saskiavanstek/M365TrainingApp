using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<GeneratedLink> GeneratedLinks { get; set; }
}

public class GeneratedLink
{
    public int Id { get; set; }
    public string UniqueId { get; set; }
    public string RepositoryId { get; set; }
    public DateTime ExpirationDate { get; set; }
}