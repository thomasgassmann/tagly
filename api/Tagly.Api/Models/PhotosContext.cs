using Microsoft.EntityFrameworkCore;

public class BloggingContext : DbContext
{
    public DbSet<StoredPhoto> Photos { get; set; }

    public string DbPath { get; }

    public BloggingContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "photos.db");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}

public class StoredPhoto
{
    public string? Id { get; set; }
    public required string FileName { get; set; }
    public required byte[] Data { get; set; }
    public DateTime? Date { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Description { get; set; }
}
