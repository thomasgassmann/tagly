using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using Tagly.App.Models;

namespace Tagly.App.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private string? _currentDescription;
    private string? _currentFileName;
    private string? _searchLocation;
    private double? _currentLatitude;
    private double? _currentLongitude;
    private DateTimeOffset? _currentDate;
    private TimeSpan? _currentTime;

    public ObservableCollection<PhotoItem> Photos { get; } = [];

    public ObservableCollection<PhotoItem> SelectedPhotos { get; } = [];

    public string? CurrentDescription
    {
        get => _currentDescription;
        set => this.RaiseAndSetIfChanged(ref _currentDescription, value);
    }

    public string? CurrentFileName
    {
        get => _currentFileName;
        set => this.RaiseAndSetIfChanged(ref _currentFileName, value);
    }

    public string? SearchLocation
    {
        get => _searchLocation;
        set => this.RaiseAndSetIfChanged(ref _searchLocation, value);
    }

    public double? CurrentLatitude
    {
        get => _currentLatitude;
        set => this.RaiseAndSetIfChanged(ref _currentLatitude, value);
    }

    public double? CurrentLongitude
    {
        get => _currentLongitude;
        set => this.RaiseAndSetIfChanged(ref _currentLongitude, value);
    }

    public DateTimeOffset? CurrentDate
    {
        get => _currentDate;
        set => this.RaiseAndSetIfChanged(ref _currentDate, value);
    }

    public TimeSpan? CurrentTime
    {
        get => _currentTime;
        set => this.RaiseAndSetIfChanged(ref _currentTime, value);
    }

    public DateTimeOffset? CurrentDateTimeOffset =>
        GetDateTimeOffset(CurrentDate, CurrentTime);

    public static DateTimeOffset? GetDateTimeOffset(DateTimeOffset? date, TimeSpan? time)
    {
        if (date is null)
        {
            return null;
        }

        return time is null ? date.Value.Date.AddHours(12) : date.Value.Date.AddSeconds(time.Value.TotalSeconds);
    }
}