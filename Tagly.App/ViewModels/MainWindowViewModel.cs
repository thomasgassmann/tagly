using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using Tagly.App.Models;

namespace Tagly.App.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private string? _currentDescription;
    private string? _currentFileName;
    private double? _currentLatitude;
    private double? _currentLongitude;
    private DateTimeOffset? _currentDate;

    public ObservableCollection<PhotoItem> Photos { get; } = [];

    public ObservableCollection<PhotoItem> SelectedPhotos { get; } = [];

    public string? CurrentDescription
    {
        get => _currentDescription;
        set { this.RaiseAndSetIfChanged(ref _currentDescription, value); }
    }

    public string? CurrentFileName
    {
        get => _currentFileName;
        set => this.RaiseAndSetIfChanged(ref _currentFileName, value);
    }

    public double? CurrentLatitude
    {
        get => _currentLatitude;
        set { this.RaiseAndSetIfChanged(ref _currentLatitude, value); }
    }

    public double? CurrentLongitude
    {
        get => _currentLongitude;
        set { this.RaiseAndSetIfChanged(ref _currentLongitude, value); }
    }

    public DateTimeOffset? CurrentDate
    {
        get => _currentDate;
        set { this.RaiseAndSetIfChanged(ref _currentDate, value); }
    }
}