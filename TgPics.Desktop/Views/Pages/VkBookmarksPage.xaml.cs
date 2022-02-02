namespace TgPics.Desktop.Views.Pages;

using Microsoft.UI.Xaml.Controls;
using TgPics.Desktop.MVVM.Interfaces;
using TgPics.Desktop.ViewModels.Pages;

public sealed partial class VkBookmarksPage
    : Page, IViewModel<VkBookmarksViewModel>
{
    public VkBookmarksPage()
    {
        InitializeComponent();
        ViewModel = new();
    }

    public VkBookmarksViewModel ViewModel { get; set; }
}