namespace TgPics.Desktop.Views.Pages;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Linq;
using System.Windows.Input;
using TgPics.Desktop.Helpers.Interfaces;
using TgPics.Desktop.MVVM.Interfaces;
using TgPics.Desktop.ViewModels;
using TgPics.Desktop.ViewModels.Pages;

public sealed partial class PrepareToPublishPage : Page,
    IViewModel<PrepareToPublishViewModel>,
    IExternalNavigation,
    IActionable
{
    public PrepareToPublishPage()
    {
        InitializeComponent();
    }

    public PrepareToPublishViewModel ViewModel { get; set; }

    public string ActionName => "Добавить в очередь";
    public ICommand Action => ViewModel.PublishCommand;

    public void OnExternalNavigatedTo(object parameter)
    {
        if (parameter is PrepareToPublishPostViewModel post)
            ViewModel = new(post);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        OnExternalNavigatedTo(e.Parameter);
    }

    // бинд на SelectedItems не работает...
    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e) =>
        ViewModel.SelectedPhotos = (sender as ListView)
        .SelectedItems.Select(i => i as PrepareToPublishPhotoViewModel)
        .ToList();
}