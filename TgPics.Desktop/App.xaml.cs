namespace TgPics.Desktop;

using DesktopKit.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TgPics.Desktop.Helpers.Interfaces;
using TgPics.Desktop.Values;
using TgPics.Desktop.ViewModels.Pages;
using TgPics.Desktop.ViewModels.Windows;
using TgPics.Desktop.Views.Pages;
using TgPics.Desktop.Views.Windows;

public partial class App : Application
{
    public App()
    {
        Services = ConfigureServices();
        ConfigureNavigation(Services.GetRequiredService<INavigationService>());
        InitializeComponent();
    }

    private static MainWindow mainWindow;

    public new static App Current => (App)Application.Current;
    public IServiceProvider Services { get; private set; }


    //public async static Task ShowDialog(string pageId, object parameter = null)
    //{
    //    var info = MainWindow.PageTagToTypeAndTitle(pageId);
    //
    //    var instanse = Activator.CreateInstance(info.Item1);
    //
    //    if (instanse is IExternalNavigation page)
    //        page.OnExternalNavigatedTo(parameter);
    //
    //    var scroll = new ScrollViewer
    //    {
    //        Content = instanse,
    //        Padding = new Thickness(0, 0, 4, 0),
    //        HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
    //        VerticalScrollBarVisibility = ScrollBarVisibility.Auto
    //    };
    //
    //    var dialog = new ContentDialog()
    //    {
    //        XamlRoot = XamlRoot,
    //        Title = info.Item2,
    //        Content = scroll,
    //        CloseButtonText = "Отмена",
    //    };
    //
    //    if (instanse is IActionable actionable)
    //    {
    //        dialog.IsPrimaryButtonEnabled = true;
    //        dialog.PrimaryButtonText = actionable.ActionName;
    //        dialog.PrimaryButtonCommand = actionable.Action;
    //        dialog.DefaultButton = ContentDialogButton.Primary;
    //    }
    //
    //    await dialog.ShowAsync();
    //}

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        mainWindow = new MainWindow();
        mainWindow.Activate();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<INavigationService, NavigationService>();

        services.AddTransient<IMainWindowVM, MainWindowVM>();
        services.AddTransient<IVkBookmarksVM, VkBookmarksVM>();
        services.AddTransient<ISettingsVM, SettingsVM>();

        return services.BuildServiceProvider();
    }

    private static void ConfigureNavigation(INavigationService navigation)
    {
        navigation.PageTypes = new Dictionary<string, Type>
        {
            { Pages.SETTINGS, typeof(SettingsPage) },
            { Pages.VK_BOOKMARKS, typeof(VkBookmarksPage) }
        };

        navigation.NotFoundPage = typeof(NotFoundPage);
    }
}