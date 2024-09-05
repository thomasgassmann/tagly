using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Tagly.App;

public static class Utils
{
    public static async Task ShowMessageAsync(this Window window, string title, string text)
    {
        await MessageBoxManager.GetMessageBoxStandard(title, text, ButtonEnum.Ok).ShowWindowDialogAsync(window);
    }
}
