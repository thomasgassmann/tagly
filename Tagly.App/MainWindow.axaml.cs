using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI.Avalonia;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Tagly.App.Models;
using Tagly.App.ViewModels;
using Tagly.Grpc;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
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
        var files = Directory.GetFiles(_sourcePath).OrderBy(x => 
        {
            var regex = Regex.Matches(x, @"\((\d+)\)");
            if (!regex.Any())
            {
                return 0;
            }

            var lastMatch = regex.Last();
            if (lastMatch.Success && lastMatch.Groups.Count > 1 && int.TryParse(lastMatch.Groups[1].Value, out int i))
            {
                return i;
            }

            return 0;
        }).ThenBy(x => x);
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
        var target = Path.Combine(_backupPath, fileName);
        while (Path.Exists(target))
        {
            target = Path.Combine(_backupPath, Guid.NewGuid() + fileName);
        }
        
        File.Copy(path, target);
    }

    private async void SendClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedPhotos.Count == 0)
        {
            await this.ShowMessageAsync("None selected", "Please select a photo first");
            return;
        }

        var selectedItems = _viewModel.SelectedPhotos.ToList();
        var box = MessageBoxManager.GetMessageBoxStandard(
            Tagly.App.Resources.ConfirmSend, 
            string.Format(Tagly.App.Resources.ConfirmSendText, selectedItems.Count), 
            ButtonEnum.YesNo);
        var result = await box.ShowWindowDialogAsync(this);
        if (result != ButtonResult.Yes)
        {
            return;
        }

        IsEnabled = false;
        var failure = false;
        foreach (var selected in selectedItems)
        {
            try
            {
                BackupPhoto(selected.FilePath);
                var content = await File.ReadAllBytesAsync(selected.FilePath);
                var byteString = ByteString.CopyFrom(content);

                var dateTime = selected.Date.GetValueOrDefault().LocalDateTime;
                var response = await _client.Client.AddPhotoAsync(new ServerPhoto
                {
                    Data = byteString,
                    Meta = new ServerPhotoMeta
                    {
                        Latitude = selected.Latitude.GetValueOrDefault(),
                        Longitude = selected.Longitude.GetValueOrDefault(),
                        Date = Timestamp.FromDateTime(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)),
                        Description = selected.Description ?? string.Empty,
                        FileName = selected.FileName,
                    }
                });

                if (!response.Success)
                {
                    await this.ShowMessageAsync(Tagly.App.Resources.Failure, selected.FilePath);
                    failure = true;
                    break;
                }

                File.Delete(selected.FilePath);
                _viewModel.Photos.Remove(selected);
                _viewModel.SelectedPhotos.Remove(selected);
                CurrentImage.Source = null;
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync(Tagly.App.Resources.Failure, ex.Message);
                break;
            }
        }
        
        IsEnabled = true;
        if (!failure)
        {
            await this.ShowMessageAsync(Tagly.App.Resources.Success, Tagly.App.Resources.SuccessfullySent);
        }
    }

    private async void LoadFiles(object? sender, RoutedEventArgs e)
    {
        UpdateFilesFromSource();
        await this.ShowMessageAsync(Tagly.App.Resources.Success, Tagly.App.Resources.SuccessfullyLoaded);
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e) =>
        UpdateFilesFromSource();

    private void GoHomeClick(object? sender, RoutedEventArgs e)
    {
        var location = SphericalMercator.FromLonLat(new MPoint(_defaultLongitude, _defaultLatitude));
        
        _mapControl.Map.Navigator.CenterOnAndZoomTo(location, 1);
    }

    private async void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_viewModel.SelectedPhotos.Count != 1)
        {
            CurrentImage.Source = null;

            _viewModel.CurrentFileName = string.Join(", ", _viewModel.SelectedPhotos.Select(x => x.FileName).ToArray());
            return;
        }

        var photo = _viewModel.SelectedPhotos.First();
        try
        {
            CurrentImage.Source = new Bitmap(photo.FilePath);
            
            _viewModel.CurrentDate = photo.Date;
            _viewModel.CurrentLatitude = photo.Latitude;
            _viewModel.CurrentLongitude = photo.Longitude;
            _viewModel.CurrentDescription = photo.Description;
            _viewModel.CurrentFileName = photo.FileName;
        }
        catch (Exception ex)
        {
            await this.ShowMessageAsync(Tagly.App.Resources.Failure, ex.Message);
        }
    }

    private void DescriptionChanged(object? sender, TextChangedEventArgs e)
    {
        if (_viewModel.SelectedPhotos.Count != 1)
        {
            return;
        }
        
        MutateSelected(item => item.Description = _viewModel.CurrentDescription);
    }

    private void DateChanged(object? sender, DatePickerSelectedValueChangedEventArgs e)
    {
        if (_viewModel.SelectedPhotos.Count != 1)
        {
            return;
        }
        
        MutateSelected(item => item.Date = MainWindowViewModel.GetDateTimeOffset(e.NewDate, _viewModel.CurrentTime));
    }

    private void LongitudeChanged(object? sender, TextChangedEventArgs e)
    {
        if (_viewModel.SelectedPhotos.Count != 1)
        {
            return;
        }
        
        MutateSelected(item => item.Longitude = _viewModel.CurrentLongitude);
    }

    private void LatitudeChanged(object? sender, TextChangedEventArgs e)
    {
        if (_viewModel.SelectedPhotos.Count != 1)
        {
            return;
        }
        
        MutateSelected(item => item.Latitude = _viewModel.CurrentLatitude);
    }

    private void TimeChanged(object? sender, TimePickerSelectedValueChangedEventArgs e)
    {
        if (_viewModel.SelectedPhotos.Count != 1)
        {
            return;
        }

        MutateSelected(item => item.Date = MainWindowViewModel.GetDateTimeOffset(_viewModel.CurrentDate, e.NewTime));
    }

    private void MutateSelected(Action<PhotoItem> action)
    {
        foreach (var photo in _viewModel.SelectedPhotos)
        {
            action(photo);
        }
    }

    private void ApplyToSelected(object? sender, RoutedEventArgs e)
    {
        MutateSelected(item =>
        {
            item.Latitude = _viewModel.CurrentLatitude;
            item.Longitude = _viewModel.CurrentLongitude;
            item.Date = _viewModel.CurrentDateTimeOffset;
            item.Description = _viewModel.CurrentDescription;
        });
    }

    private async void ResetSelected(object? sender, RoutedEventArgs e)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(
            Tagly.App.Resources.ConfirmReset, 
            string.Format(Tagly.App.Resources.ConfirmResetText, _viewModel.SelectedPhotos.Count), 
            ButtonEnum.YesNo);
        var result = await box.ShowWindowDialogAsync(this);
        if (result != ButtonResult.Yes)
        {
            return;
        }
        
        MutateSelected(item =>
        {
            item.Latitude = null;
            item.Longitude = null;
            item.Date = null;
            item.Description = null;
        });
    }

    private async void GoSearchLocation(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_viewModel.SearchLocation))
        {
            return;
        }

        var resultingLocation = await GeoCode.GetPointAsync(this._viewModel.SearchLocation);
        if (resultingLocation != null)
        {
            _mapControl.Map.Navigator.CenterOnAndZoomTo(resultingLocation, 1);
        }
        else
        {
            await this.ShowMessageAsync(Tagly.App.Resources.Failure, Tagly.App.Resources.GeolocationFailure);
        }
    }
}
