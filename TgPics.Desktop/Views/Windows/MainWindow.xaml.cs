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

    public void Navigate(string pageId, object parameter = null)
    {
        var info = PageTagToTypeAndTitle(pageId);
        frame.Navigate(info.Item1, parameter);
        currentPageTitleTextBlock.Text = $"🧭 {info.Item2}";
    }

    public static (Type, string) PageTagToTypeAndTitle(string tag) =>
        tag switch
        {
            "settings" => (typeof(SettingsPage), "Настройки"),
            "vk_bookmarks" => (typeof(VkBookmarksPage), "ВКонтакте: Закладки"),
            "prepare_to_publish" => (typeof(PrepareToPublishPage), "Подготовка публикации"),
            _ => (typeof(PageNotFound), "404: Страница не найдена"),
        };

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
            Navigate(args.InvokedItemContainer.Tag?.ToString());
    }
}
