namespace TgPics.Desktop.Views.Pages;

using Microsoft.UI.Xaml.Controls;
using TgPics.Desktop.MVVM.Interfaces;
using TgPics.Desktop.ViewModels.Pages;

public sealed partial class VkBookmarksPage
    : Page, IViewModel<VkBookmarksPageViewModel>
{
    public VkBookmarksPage()
    {
        InitializeComponent();
        ViewModel = new();
    }

    public VkBookmarksPageViewModel ViewModel { get; set; }
}