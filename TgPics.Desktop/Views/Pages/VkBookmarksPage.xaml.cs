namespace TgPics.Desktop.Views.Pages;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using TgPics.Desktop.MVVM.Interfaces;
using TgPics.Desktop.ViewModels.Pages;

public sealed partial class VkBookmarksPage
    : Page, IViewModel<IVkBookmarksVM>
{
    public VkBookmarksPage()
    {
        ViewModel = App.Current.Services.GetService<IVkBookmarksVM>();
        InitializeComponent();
    }

    public IVkBookmarksVM ViewModel { get; set; }
}