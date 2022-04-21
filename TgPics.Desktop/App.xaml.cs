namespace TgPics.Desktop;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TgPics.Desktop.Helpers.Interfaces;
using TgPics.Desktop.Views.Windows;

public partial class App : Application
{
    public App() => InitializeComponent();

    private static MainWindow mainWindow;

    public static XamlRoot XamlRoot { get; set; }

    public static void NavgateTo(string pageId, object parameter = null) =>
        mainWindow.Navigate(pageId, parameter);

    public async static Task ShowDialog(string pageId, object parameter = null)
    {
        var info = MainWindow.PageTagToTypeAndTitle(pageId);

        var instanse = Activator.CreateInstance(info.Item1);

        if (instanse is IExternalNavigation page)
            page.OnExternalNavigatedTo(parameter);

        var scroll = new ScrollViewer
        {
            Content = instanse,
            Padding = new Thickness(0, 0, 4, 0),
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        var dialog = new ContentDialog()
        {
            XamlRoot = XamlRoot,
            Title = info.Item2,
            Content = scroll,
            CloseButtonText = "Отмена",
        };

        if (instanse is IActionable actionable)
        {
            dialog.IsPrimaryButtonEnabled = true;
            dialog.PrimaryButtonText = actionable.ActionName;
            dialog.PrimaryButtonCommand = actionable.Action;
            dialog.DefaultButton = ContentDialogButton.Primary;
        }

        await dialog.ShowAsync();
    }

    public async static Task ShowExceptionDialog(Exception ex)
    {
        var content = ex.Message;

#if DEBUG
        content = ex.ToString();
#endif

        var dialog = new ContentDialog()
        {
            XamlRoot = XamlRoot,
            Title = "Ошибка 😢",
            Content = content,
            CloseButtonText = "ОК",
        };

        await dialog.ShowAsync();
    }

    public async static Task ShowSuccessfulDialog(string msg)
    {
        var dialog = new ContentDialog()
        {
            XamlRoot = XamlRoot,
            Title = "Успех! 🤩",
            Content = msg,
            CloseButtonText = "ОК",
        };

        await dialog.ShowAsync();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        mainWindow = new MainWindow();
        mainWindow.Activate();

        Trace.Listeners.Add(new TextWriterTraceListener("system.net.log", "System.Net"));
    }
}