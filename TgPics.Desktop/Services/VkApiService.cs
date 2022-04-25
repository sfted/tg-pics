namespace TgPics.Desktop.Services;

using DesktopKit.Services;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using TgPics.Desktop.Helpers;
using TgPics.Desktop.Values;
using TgPics.Desktop.Views.Pages;
using VkNet;
using VkNet.Model;

internal interface IVkApiService
{
    VkApi Api { get; }
    bool IsAuthorized { get; }
    bool HasSavedToken { get; }

    Task AuthorizeAsync();
    Task AuthorizeFromSavedAsync();
    void LogOut();
}

internal class VkApiService : IVkApiService
{
    public VkApiService(
        INavigationService navigationService,
        ISettingsService settingsService)
    {
        this.navigationService = navigationService;
        this.settingsService = settingsService;
        api = new();
    }

    readonly INavigationService navigationService;
    readonly ISettingsService settingsService;
    readonly VkApi api;

    public VkApi Api => api;
    public bool IsAuthorized => api.IsAuthorized;
    public bool HasSavedToken =>
        !string.IsNullOrEmpty(settingsService.Get<string>(SettingsKeys.VK_TOKEN));

    public async Task AuthorizeFromSavedAsync()
    {
        var token = settingsService.Get<string>(SettingsKeys.VK_TOKEN);

        await api.AuthorizeAsync(new ApiAuthParams
        {
            AccessToken = token,
        });
    }

    public async Task AuthorizeAsync()
    {
        try
        {
            var page = new VkLoginPage();

            var dialog = new ContentDialog()
            {
                Title = "Вход в аккаунт ВК",
                Content = page,
                CloseButtonText = "Отмена"
            };

            page.ViewModelLoaded += () =>
            {
                page.ViewModel.LoginCompleted += async () =>
                {
                    dialog.Hide();
                    settingsService.Set(SettingsKeys.VK_TOKEN, page.ViewModel.Token);
                    await AuthorizeFromSavedAsync();
                };
            };

            await navigationService.ShowDialogAsync(dialog);
        }
        catch (Exception ex)
        {
            await navigationService.ShowDialogAsync(new ContentDialog().AsError(ex));
        }
    }

    public void LogOut()
    {
        settingsService.Set(SettingsKeys.VK_TOKEN, string.Empty);
    }
}
