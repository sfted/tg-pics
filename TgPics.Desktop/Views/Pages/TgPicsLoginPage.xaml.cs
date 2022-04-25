namespace TgPics.Desktop.Views.Pages;

using DesktopKit.MVVM.Interfaces;
using Microsoft.UI.Xaml.Controls;
using TgPics.Desktop.ViewModels.Pages;

internal sealed partial class TgPicsLoginPage : Page, IViewModel<TgPicsLoginPageVM>
{
    public TgPicsLoginPage()
    {
        InitializeComponent();
        ViewModel = new();
    }

    public TgPicsLoginPageVM ViewModel { get; set; }
}
