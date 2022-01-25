using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TgPics.Desktop.Views.Pages;

namespace TgPics.Desktop.Views.Windows
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnFrameLoaded(object sender, RoutedEventArgs e)
        {
            frame.Navigate(typeof(SettingsPage));
        }

        private void OnGridLoaded(object sender, RoutedEventArgs e)
        {
            App.XamlRoot = (sender as Grid).XamlRoot;
        }
    }
}
