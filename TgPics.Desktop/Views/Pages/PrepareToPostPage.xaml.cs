namespace TgPics.Desktop.Views.Pages;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Linq;
using TgPics.Desktop.MVVM.Interfaces;
using TgPics.Desktop.ViewModels;
using TgPics.Desktop.ViewModels.Pages;

public sealed partial class PrepareToPublishPage : Page, IViewModel<PrepareToPublishViewModel>
{
    public PrepareToPublishPage()
    {
        InitializeComponent();
    }

    public PrepareToPublishViewModel ViewModel { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is PrepareToPublishPostViewModel post)
            ViewModel = new(post);
    }

    // бинд на SelectedItems не работает...
    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e) =>
        ViewModel.SelectedPhotos = (sender as ListView)
        .SelectedItems.Select(i => i as PrepareToPublishPhotoViewModel)
        .ToList();
}