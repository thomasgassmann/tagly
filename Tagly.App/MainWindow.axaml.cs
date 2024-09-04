using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Layout;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI.Avalonia;
using NetTopologySuite.Geometries;

namespace Tagly.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
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
        DataContext = this;

        // Load some example data
        Files.Add(new FileItem { Filename = "file1.txt", Latitude = 34.0522, Longitude = -118.2437, Date = "2024-09-01", Description = "Example file 1" });
        Files.Add(new FileItem { Filename = "file2.txt", Latitude = 40.7128, Longitude = -74.0060, Date = "2024-09-02", Description = "Example file 2" });
    }
    
    public ObservableCollection<FileItem> Files { get; set; } = new ObservableCollection<FileItem>();
    
    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FileList.SelectedItem is FileItem selectedFile)
        {
            FilenameTextBox.Text = selectedFile.Filename;
            LatitudeTextBox.Text = selectedFile.Latitude.ToString();
            LongitudeTextBox.Text = selectedFile.Longitude.ToString();
            DescriptionTextBox.Text = selectedFile.Description;
        }
    }
    
}

public class FileItem
{
    public string Filename { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Date { get; set; }
    public string Description { get; set; }
}