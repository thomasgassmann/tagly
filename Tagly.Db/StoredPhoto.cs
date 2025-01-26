using System.ComponentModel.DataAnnotations.Schema;

namespace Tagly.Db;

public class StoredPhoto
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public required string FileName { get; set; }
    public required byte[] Data { get; set; }
    public DateTime? Date { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Description { get; set; }
    public DateTime? Created { get; set; }
}