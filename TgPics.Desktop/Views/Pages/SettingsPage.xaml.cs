namespace TgPics.Desktop.Views.Pages;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using TgPics.Desktop.MVVM.Interfaces;
using TgPics.Desktop.ViewModels.Pages;

public sealed partial class SettingsPage : Page, IViewModel<ISettingsVM>
{
    public SettingsPage()
    {
        InitializeComponent();
        ViewModel = App.Current.Services.GetService<ISettingsVM>();
    }

    public ISettingsVM ViewModel { get; set; }
}