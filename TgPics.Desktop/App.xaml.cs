namespace TgPics.Desktop;

using Microsoft.UI.Xaml;
using TgPics.Desktop.Views.Windows;

public partial class App : Application
{
    public App() => InitializeComponent();

    private MainWindow mainWindow;

    public static XamlRoot XamlRoot { get; set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        mainWindow = new MainWindow();
        mainWindow.Activate();
    }
}