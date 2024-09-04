using Microsoft.EntityFrameworkCore;

namespace Tagly.Db;

public class TaglyContext : DbContext
{
    public DbSet<StoredPhoto> Photos { get; set; }

    public TaglyContext(DbContextOptions<TaglyContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite();
}

