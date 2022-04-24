namespace TgPics.Desktop.Views.Pages;

using DesktopKit.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Linq;
using System.Windows.Input;
using TgPics.Desktop.Helpers.Interfaces;
using TgPics.Desktop.MVVM.Interfaces;
using TgPics.Desktop.Services;
using TgPics.Desktop.ViewModels;
using TgPics.Desktop.ViewModels.Pages;

internal sealed partial class PrepareToPublishPage : Page,
    IViewModel<PrepareToPublishVM>,
    IExternalNavigation,
    IActionable
{
    public PrepareToPublishPage()
    {
        navigationService = App.Current.Services.GetService<INavigationService>();
        settingsService = App.Current.Services.GetService<ISettingsService>();
        InitializeComponent();
    }

    readonly INavigationService navigationService;
    readonly ISettingsService settingsService;

    public PrepareToPublishVM ViewModel { get; set; }

    public string ActionName => "Добавить в очередь";
    public ICommand Action => ViewModel.PublishCommand;

    public void OnExternalNavigatedTo(object parameter)
    {
        if (parameter is PrepareToPublishPostViewModel post)
            ViewModel = new(navigationService, settingsService, post);
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