namespace TgPics.Desktop.Views.Windows;

using DesktopKit.MVVM.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using TgPics.Desktop.ViewModels.Windows;

internal sealed partial class MainWindow : Window, IViewModel<IMainWindowVM>
{
    public MainWindow()
    {
        ViewModel = App.Current.Services.GetService<IMainWindowVM>();
        Title = "tg-pics";
        InitializeComponent();
    }

    private IMainWindowVM viewModel;

    public IMainWindowVM ViewModel
    {
        get => viewModel;
        set
        {
            viewModel = value;
            viewModel.NavigationService.Navigated += OnNavigated;
        }
    }

    private void OnGridLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.NavigationService.XamlRoot = (sender as Grid).XamlRoot;
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(titleBar);
    }

    private void OnNavigated(Type page, object parameter)
    {
        frame.Navigate(page, parameter, new DrillInNavigationTransitionInfo());
    }

    private void OnMenuToggleButtonClick(object sender, RoutedEventArgs e)
    {
        navigationView.IsPaneOpen = !navigationView.IsPaneOpen;
    }

    private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer != null)
            ViewModel.NavigationService.NavigateTo(args.InvokedItemContainer.Tag?.ToString());
    }
}
