using Microsoft.EntityFrameworkCore;

namespace Tagly.Db;

public class TaglyContext : DbContext
{
    public DbSet<StoredPhoto> Photos { get; set; }

    private readonly string? _dbPath;

    public TaglyContext(string dbPath)
    {
        _dbPath = dbPath;
    }

    public TaglyContext(DbContextOptions<TaglyContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!string.IsNullOrEmpty(_dbPath))
        {
            options.UseSqlite($"Data Source={_dbPath}");
        }
        else
        {
            options.UseSqlite();
        }
    }
}