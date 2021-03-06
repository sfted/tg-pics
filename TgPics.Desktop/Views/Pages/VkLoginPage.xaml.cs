namespace TgPics.Desktop.Views.Pages;

using DesktopKit.MVVM.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using TgPics.Desktop.Services;
using TgPics.Desktop.Values;
using TgPics.Desktop.ViewModels.Pages;

public sealed partial class VkLoginPage : Page, IViewModel<VkLoginPageVM>
{
    public VkLoginPage()
    {
        settingsService = App.Current.Services.GetService<ISettingsService>();
        InitializeComponent();
    }

    readonly ISettingsService settingsService;

    public event Action ViewModelLoaded;

    public VkLoginPageVM ViewModel { get; set; }

    private void OnNavigationStarting(
        WebView2 sender,
        CoreWebView2NavigationStartingEventArgs args) =>
        ViewModel.ProceedLogin(args.Uri.ToString());

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        var webView2 = sender as WebView2;
        await webView2.EnsureCoreWebView2Async();

        if (string.IsNullOrEmpty(settingsService.Get<string>(SettingsKeys.VK_TOKEN)))
            webView2.CoreWebView2.CookieManager.DeleteAllCookies();

        ViewModel = new();
        ViewModelLoaded?.Invoke();
        webView2.Source = ViewModel.AuthUrl;
    }
}