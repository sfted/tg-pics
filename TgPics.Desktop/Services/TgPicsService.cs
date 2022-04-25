namespace TgPics.Desktop.Services;

using DesktopKit.MVVM;
using DesktopKit.Services;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using TgPics.Api.Client;
using TgPics.Core.Models.Requests;
using TgPics.Desktop.Helpers;
using TgPics.Desktop.Values;
using TgPics.Desktop.Views.Pages;

internal interface ITgPicsService
{
    TgPicsApi Api { get; }
    bool HasSavedToken { get; }
    bool IsAuthorized { get; }

    Task AuthorizeAsync();
    void AuthorizeFromSaved();
    void LogOut();
}

internal class TgPicsService : ITgPicsService
{
    public TgPicsService(
        INavigationService navigationService,
        ISettingsService settingsService)
    {
        this.navigationService = navigationService;
        this.settingsService = settingsService;
    }

    readonly INavigationService navigationService;
    readonly ISettingsService settingsService;
    TgPicsApi api;

    public TgPicsApi Api => api;
    public bool IsAuthorized { get; private set; }
    public bool HasSavedToken =>
        !string.IsNullOrEmpty(settingsService.Get<string>(SettingsKeys.TG_PICS_TOKEN));

    public void AuthorizeFromSaved()
    {
        var host = settingsService.Get<string>(SettingsKeys.TG_PICS_HOST);
        var token = settingsService.Get<string>(SettingsKeys.TG_PICS_TOKEN);

        api = new(host, token, false);
        IsAuthorized = true;
    }

    public async Task AuthorizeAsync()
    {
        //try
        //{
        var page = new TgPicsLoginPage();

        var host = settingsService.Get<string>(SettingsKeys.TG_PICS_HOST);
        var username = settingsService.Get<string>(SettingsKeys.TG_PICS_USERNAME);

        if (!string.IsNullOrEmpty(host))
            page.ViewModel.Host = host;

        if (!string.IsNullOrEmpty(username))
            page.ViewModel.Username = username;

        var dialog = new ContentDialog()
        {
            Title = "Вход в аккаунт",
            Content = page,
            PrimaryButtonText = "Войти",
            PrimaryButtonCommand = new Command<TgPicsLoginPage>(ProceedLogin),
            DefaultButton = ContentDialogButton.Primary,
            CloseButtonText = "Отмена",
            PrimaryButtonCommandParameter = page
        };

        await navigationService.ShowDialogAsync(dialog);
        //}
        //catch (Exception ex)
        //{
        //    await navigationService.ShowDialogAsync(new ContentDialog().AsError(ex));
        //}
    }

    public void LogOut()
    {
        settingsService.Set(SettingsKeys.TG_PICS_TOKEN, string.Empty);
    }

    private async void ProceedLogin(TgPicsLoginPage page)
    {
        try
        {
            var client = new TgPicsApi(page.ViewModel.Host, secure: false);
            var request = new UsersAuthRequest
            {
                Username = page.ViewModel.Username,
                Password = page.ViewModel.Password
            };

            var response = await client.AuthAsync(request);
            settingsService.Set(SettingsKeys.TG_PICS_HOST, page.ViewModel.Host);
            settingsService.Set(SettingsKeys.TG_PICS_TOKEN, response.Token);
            settingsService.Set(SettingsKeys.TG_PICS_USERNAME, response.Username);
            IsAuthorized = true;
        }
        catch (Exception ex)
        {
            await navigationService.ShowDialogAsync(new ContentDialog().AsError(ex));
        }
    }

}
