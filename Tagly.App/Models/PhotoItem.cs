using System;
using System.IO;

namespace Tagly.App.Models;

public class PhotoItem
{
    public required string FilePath { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime? Date { get; set; }
    public string? Description { get; set; }
    
    public string FileName => Path.GetFileName(FilePath);
}