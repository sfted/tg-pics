namespace TgPics.Desktop.Views.Pages;

using DesktopKit.MVVM.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using TgPics.Desktop.ViewModels.Pages;

internal sealed partial class VkBookmarksPage
    : Page, IViewModel<IVkBookmarksVM>
{
    public VkBookmarksPage()
    {
        ViewModel = App.Current.Services.GetService<IVkBookmarksVM>();
        InitializeComponent();
    }

    public IVkBookmarksVM ViewModel { get; set; }
}