namespace TgPics.Desktop.ViewModels.Windows;

using DesktopKit.MVVM;
using DesktopKit.Services;

internal interface IMainWindowVM
{
    public INavigationService NavigationService { get; }
}

internal class MainWindowVM : ViewModelBase, IMainWindowVM
{
    public MainWindowVM(INavigationService navigationService)
    {
        NavigationService = navigationService;
    }

    public INavigationService NavigationService { get; private set; }
}
