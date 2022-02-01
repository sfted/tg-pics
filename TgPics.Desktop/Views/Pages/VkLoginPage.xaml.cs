﻿namespace TgPics.Desktop.Views.Pages;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using TgPics.Desktop.MVVM.Interfaces;
using TgPics.Desktop.ViewModels.Pages;

public sealed partial class VkLoginPage : Page, IViewModel<VkLoginPageViewModel>
{
    public VkLoginPage()
    {
        InitializeComponent();
    }

    public event Action ViewModelLoaded;

    public VkLoginPageViewModel ViewModel { get; set; }

    private void OnNavigationStarting(
        WebView2 sender,
        CoreWebView2NavigationStartingEventArgs args) =>
        ViewModel.ProceedLogin(args.Uri.ToString());

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        var webView2 = sender as WebView2;
        await webView2.EnsureCoreWebView2Async();
        webView2.CoreWebView2.CookieManager.DeleteAllCookies();

        ViewModel = new();
        ViewModelLoaded?.Invoke();
        webView2.Source = ViewModel.AuthUrl;
    }
}