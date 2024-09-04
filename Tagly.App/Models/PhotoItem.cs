using System;
using System.ComponentModel;
using System.IO;
using ReactiveUI;

namespace Tagly.App.Models;

public class PhotoItem : ReactiveObject
{
    private string? _description;
    private readonly string? _filePath;
    private double? _latitude;
    private double? _longitude;
    private DateTimeOffset? _date;

    public string? Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    public required string FilePath
    {
        get => _filePath!;
        init => this.RaiseAndSetIfChanged(ref _filePath, value);
    }

    public double? Latitude
    {
        get => _latitude;
        set
        {
            this.RaiseAndSetIfChanged(ref _latitude, value); 
            this.RaisePropertyChanged(nameof(LatitudeString));
        }
    }

    public double? Longitude
    {
        get => _longitude;
        set
        {
            this.RaiseAndSetIfChanged(ref _longitude, value);
            this.RaisePropertyChanged(nameof(LongitudeString));
        }
    }

    public DateTimeOffset? Date
    {
        get => _date;
        set
        {
            this.RaiseAndSetIfChanged(ref _date, value);
            this.RaisePropertyChanged(nameof(DateString));
        }
    }

    public string DateString => Date?.ToString("dd.MM.yyyy") ?? "";
    public string LongitudeString => $"{Longitude:0.####}";
    public string LatitudeString => $"{Latitude:0.####}";
    public string FileName => Path.GetFileName(FilePath);
}