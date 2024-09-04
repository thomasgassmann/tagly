using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            Console.WriteLine(mPoint);
            layer?.DataHasChanged();
        };
        DataContext = _viewModel;
    }

    private void LoadClick(object? sender, RoutedEventArgs e)
    {
        
    }

    private void ApplyToSelectedClick(object? sender, RoutedEventArgs e)
    {
        
    }

    private async void SendAllClick(object? sender, RoutedEventArgs e)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Confirm", "Alle senden?", ButtonEnum.YesNo);
        var result = await box.ShowAsync();
        if (result == ButtonResult.Yes)
        {

        }
    }
}
