namespace TgPics.Desktop.Views.Pages;

using DesktopKit.MVVM.Interfaces;
using Microsoft.UI.Xaml.Controls;
using TgPics.Desktop.ViewModels.Pages;

public sealed partial class LoginPage : Page, IViewModel<LoginPageViewModel>
{
    public LoginPage()
    {
        InitializeComponent();
        ViewModel = new();
    }

    public LoginPageViewModel ViewModel { get; set; }
}
