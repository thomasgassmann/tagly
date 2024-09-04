using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using NetTopologySuite.Geometries;
using Tagly.App.Models;
using Tagly.App.ViewModels;
using Tagly.Grpc;

namespace Tagly.App;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly GrpcPhotosClient _client;

    private readonly string _sourcePath;
    private readonly string _backupPath;
    
    public MainWindow(GrpcPhotosClient client, string sourcePath, string backupPath)
    {
        _sourcePath = sourcePath;
        _backupPath = backupPath;
        
        _client = client;
        _viewModel = new MainWindowViewModel();
        InitializeComponent();
        
        var mapControl = new MapControl();
        mapControl.Map?.Layers.Add(OpenStreetMap.CreateTileLayer());
        mapControl.VerticalAlignment = VerticalAlignment.Stretch;
        MapControl.Content = mapControl;
        
        var map = mapControl.Map!;
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

    private async void SendAllClick(object? sender, RoutedEventArgs e)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Confirm", "Alle senden?", ButtonEnum.YesNo);
        var result = await box.ShowAsync();
        if (result == ButtonResult.Yes)
        {

        }
    }

    private async void LoadFiles(object? sender, RoutedEventArgs e) =>
        UpdateFilesFromSource();

    private async void Control_OnLoaded(object? sender, RoutedEventArgs e) =>
        UpdateFilesFromSource();
}
