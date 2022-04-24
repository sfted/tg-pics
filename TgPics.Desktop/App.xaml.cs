namespace TgPics.Desktop;

using DesktopKit.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using TgPics.Desktop.Services;
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

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        mainWindow = new MainWindow();
        mainWindow.Activate();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IVkApiService, VkApiService>();

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