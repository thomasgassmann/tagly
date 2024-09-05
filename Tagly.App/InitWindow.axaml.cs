using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.Configuration;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Tagly.App;

public partial class InitWindow : Window
{
    private readonly double _defaultLatitude = 0;
    private readonly double _defaultLongitude = 0;
    private readonly bool _secure = true;
    
    public InitWindow()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        InitializeComponent();

        Source.Text = config["DefaultSourcePath"];
        Backup.Text = config["DefaultBackupPath"];
        Url.Text = config["DefaultUrl"];
        _defaultLatitude = config.GetValue<double>("DefaultLatitude");
        _defaultLongitude = config.GetValue<double>("DefaultLongitude");
        _secure = config.GetValue<bool>("Secure");
    }

    private async void Login(object? sender, RoutedEventArgs e)
    {
        var source = Source.Text;
        var backup = Backup.Text;
        if (string.IsNullOrEmpty(Url.Text) || string.IsNullOrEmpty(Token.Text) || !Path.Exists(source) || !Path.Exists(backup))
        {
            return;
        }

        var client = new GrpcPhotosClient(Url.Text, Token.Text, _secure);
        try
        {
            await client.GetJwtAsync();
        }
        catch (Exception ex)
        {
            await MessageBoxManager.GetMessageBoxStandard("Failure", ex.Message, ButtonEnum.Ok).ShowAsync();
            return;
        }

        Hide();
        new MainWindow(client, source, backup, _defaultLatitude, _defaultLongitude).Show();
    }

    private async Task SelectDirectory(TextBox textBox, string title)
    {
        var result = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false,
            Title = title
        });
        if (result.Count != 1)
        {
            return;
        }

        var item = result.Single();
        if (Path.Exists(item.Path.AbsolutePath))
        {
            textBox.Text = item.Path.AbsolutePath;
        }
    }

    private async void SelectDirectorySource(object? sender, RoutedEventArgs e) =>
        await SelectDirectory(Source, "Select source");

    private async void SelectDirectoryBackup(object? sender, RoutedEventArgs e) =>
        await SelectDirectory(Backup, "Select backup");
}
