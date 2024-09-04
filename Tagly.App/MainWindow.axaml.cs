using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Grpc.Net.Client;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI.Avalonia;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using Tagly.App.Models;
using Tagly.App.ViewModels;
using Tagly.Grpc;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using Image = SixLabors.ImageSharp.Image;
using Point = NetTopologySuite.Geometries.Point;

namespace Tagly.App;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly GrpcPhotosClient _client;

    private readonly string _sourcePath;
    private readonly string _backupPath;
    private readonly double _defaultLatitude;
    private readonly double _defaultLongitude;
    
    private readonly MapControl _mapControl;
    
    public MainWindow(GrpcPhotosClient client, string sourcePath, string backupPath, double defaultLatitude, double defaultLongitude)
    {
        _sourcePath = sourcePath;
        _backupPath = backupPath;
        _defaultLatitude = defaultLatitude;
        _defaultLongitude = defaultLongitude;
        
        _client = client;
        _viewModel = new MainWindowViewModel();
        InitializeComponent();
        
        _mapControl = new MapControl();
        _mapControl.Map?.Layers.Add(OpenStreetMap.CreateTileLayer());
        _mapControl.VerticalAlignment = VerticalAlignment.Stretch;
        MapControl.Content = _mapControl;
        
        var map = _mapControl.Map!;
        var layer = new GenericCollectionLayer<List<IFeature>>
        {
            Style = SymbolStyles.CreatePinStyle()
        };
        map.Layers.Add(layer);
        map.Info += (s, e) =>
        {
            if (e.MapInfo?.WorldPosition == null)
            {
                return;
            }

            layer.Features.Clear();
            layer?.Features.Add(new GeometryFeature
            {
                Geometry = new Point(e.MapInfo.WorldPosition.X, e.MapInfo.WorldPosition.Y)
            });

            var mPoint = SphericalMercator.ToLonLat(e.MapInfo.WorldPosition.X, e.MapInfo.WorldPosition.Y);
            
            _viewModel.CurrentLatitude = mPoint.lat;
            _viewModel.CurrentLongitude = mPoint.lon;
            layer?.DataHasChanged();
        };
        DataContext = _viewModel;
    }

    private void UpdateFilesFromSource()
    {
        var currentPhotos = _viewModel.Photos.ToDictionary(x => x.FilePath, x => x);
        var files = Directory.GetFiles(_sourcePath);
        foreach (var file in files)
        {
            if (!currentPhotos.ContainsKey(file))
            {
                _viewModel.Photos.Add(new PhotoItem
                {
                    FilePath = file
                });
            }
        }
    }

    private void BackupPhoto(string path)
    {
        var fileName = Path.GetFileName(path);
        File.Copy(path, Path.Combine(_backupPath, Guid.NewGuid().ToString() + fileName));
    }

    private async void SendAllClick(object? sender, RoutedEventArgs e)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Confirm", "Alle senden?", ButtonEnum.YesNo);
        var result = await box.ShowAsync();
        if (result == ButtonResult.Yes)
        {

        }
    }

    private void LoadFiles(object? sender, RoutedEventArgs e) =>
        UpdateFilesFromSource();

    private void Control_OnLoaded(object? sender, RoutedEventArgs e) =>
        UpdateFilesFromSource();

    private void GoHomeClick(object? sender, RoutedEventArgs e)
    {
        var location = SphericalMercator.FromLonLat(new MPoint(_defaultLongitude, _defaultLatitude));
        
        _mapControl.Map.Navigator.CenterOnAndZoomTo(location, 1);
    }

    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) =>
        LoadImage();

    private void LoadImage()
    {
        if (_viewModel.SelectedPhoto == null)
        {
            return;
        }

        CurrentImage.Source = new Bitmap(_viewModel.SelectedPhoto.FilePath);
    }
}
