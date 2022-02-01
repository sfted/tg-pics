namespace TgPics.Desktop.Views.Windows;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using TgPics.Desktop.Views.Pages;

public sealed partial class MainWindow : Window
{
    public MainWindow() =>
        InitializeComponent();

    private void OnGridLoaded(object sender, RoutedEventArgs e)
    {
        App.XamlRoot = (sender as Grid).XamlRoot;

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
    }

    private void OnItemInvoked(
        NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer != null)
        {
            var info = PageTagToTypeAndTitle(args.InvokedItemContainer.Tag?.ToString());
            frame.Navigate(info.Item1);
            currentPageTitleTextBlock.Text = $"🧭 {info.Item2}";
        }
    }

    private static (Type, string) PageTagToTypeAndTitle(string tag) =>
        tag switch
        {
            "settings" => (typeof(SettingsPage), "Настройки"),
            "vk_bookmarks" => (typeof(VkBookmarksPage), "ВКонтакте: Закладки"),
            _ => (typeof(PageNotFound), "404: Страница не найдена"),
        };
}
