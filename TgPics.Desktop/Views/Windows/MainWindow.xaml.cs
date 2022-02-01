namespace TgPics.Desktop.Views.Windows;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using TgPics.Desktop.Views.Pages;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

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
        if (args.InvokedItemContainer != null)
            frame.Navigate(PageTagToType(args.InvokedItemContainer.Tag?.ToString()));
    }

    private static Type PageTagToType(string tag) =>
        tag switch
        {
            "settings" => typeof(SettingsPage),
            _ => typeof(PageNotFound),
        };
}
