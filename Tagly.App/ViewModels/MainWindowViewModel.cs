using System;
using System.Collections.ObjectModel;
using Tagly.App.Models;

namespace Tagly.App.ViewModels;

public class MainWindowViewModel
{
    public ObservableCollection<PhotoItem> Photos { get; set; } = [];
    public ObservableCollection<PhotoItem> SelectedPhotos { get; set; } = [];
    public string? CurrentDescription { get; set; }
    public string? CurrentFileName { get; set; }
    public double CurrentLatitude { get; set; }
    public double CurrentLongitude { get; set; }
    public DateTime? CurrentDate { get; set; }
}
