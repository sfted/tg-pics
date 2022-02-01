namespace TgPics.Desktop.Views.Windows;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using TgPics.Desktop.MVVM.Interfaces;
using TgPics.Desktop.ViewModels.Windows;
using TgPics.Desktop.Views.Pages;

public sealed partial class MainWindow : Window, IViewModel<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new();
    }

    public MainWindowViewModel ViewModel { get; set; }

    private void OnFrameLoaded(object sender, RoutedEventArgs e)
    {
        //frame.Navigate(typeof(SettingsPage));
    }

    private void OnGridLoaded(object sender, RoutedEventArgs e)
    {
        App.XamlRoot = (sender as Grid).XamlRoot;
    }

    private void OnItemInvoked(
        NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if(args.InvokedItemContainer != null)
            frame.Navigate(PageTagToType(args.InvokedItemContainer.Tag?.ToString()));
    }

    private static Type PageTagToType(string tag) =>
        tag switch
        {
            "settings" => typeof(SettingsPage),
            _ => typeof(PageNotFound),
        };
}
