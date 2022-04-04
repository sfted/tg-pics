namespace TgPics.Desktop;

using Microsoft.UI.Xaml;
using System.Diagnostics;
using TgPics.Desktop.Views.Windows;

public partial class App : Application
{
    public App() => InitializeComponent();

    private static MainWindow mainWindow;

    public static XamlRoot XamlRoot { get; set; }

    public static void NavgateTo(string pageId, object parameter = null) =>
        mainWindow.Navigate(pageId, parameter);

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        mainWindow = new MainWindow();
        mainWindow.Activate();

        Trace.Listeners.Add(new TextWriterTraceListener("system.net.log", "System.Net"));
    }
}