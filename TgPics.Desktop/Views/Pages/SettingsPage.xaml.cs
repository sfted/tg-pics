using Microsoft.UI.Xaml.Controls;
using TgPics.Desktop.MVVM.Interfaces;
using TgPics.Desktop.ViewModels.Pages;

namespace TgPics.Desktop.Views.Pages
{
    public sealed partial class SettingsPage : Page, IViewModel<SettingsViewModel>
    {
        public SettingsPage()
        {
            InitializeComponent();
            ViewModel = new SettingsViewModel();
        }

        public SettingsViewModel ViewModel { get; set; }
    }
}
