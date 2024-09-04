using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using Tagly.App.Models;

namespace Tagly.App.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private PhotoItem? _selectedPhoto;
    private string? _currentDescription;
    private string? _currentFileName;
    private double? _currentLatitude;
    private double? _currentLongitude;
    private DateTimeOffset? _currentDate;

    public ObservableCollection<PhotoItem> Photos { get; } = [];

    public PhotoItem? SelectedPhoto
    {
        get => _selectedPhoto;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedPhoto, value);
            if (_selectedPhoto != null)
            {
                CurrentDescription = _selectedPhoto.Description;
                CurrentFileName = _selectedPhoto.FileName;
                CurrentLatitude = _selectedPhoto.Latitude;
                CurrentLongitude = _selectedPhoto.Longitude;
                CurrentDate = _selectedPhoto.Date;
            }
        }
    }

    public string? CurrentDescription
    {
        get => _currentDescription;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentDescription, value);
            if (SelectedPhoto != null) SelectedPhoto.Description = value;
        }
    }

    public string? CurrentFileName
    {
        get => _currentFileName;
        set => this.RaiseAndSetIfChanged(ref _currentFileName, value);
    }

    public double? CurrentLatitude
    {
        get => _currentLatitude;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentLatitude, value);
            if (SelectedPhoto != null) SelectedPhoto.Latitude = value;
        }
    }

    public double? CurrentLongitude
    {
        get => _currentLongitude;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentLongitude, value);
            if (SelectedPhoto != null) SelectedPhoto.Longitude = value;
        }
    }

    public DateTimeOffset? CurrentDate
    {
        get => _currentDate;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentDate, value);
            if (SelectedPhoto != null) SelectedPhoto.Date = value;
        }
    }
}