namespace TgPics.Desktop.ViewModels.Pages;

using DesktopKit.MVVM;
using DesktopKit.Services;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using TgPics.Api.Client;
using TgPics.Core.Models.Requests;
using TgPics.Desktop.Helpers;
using TgPics.Desktop.Services;
using TgPics.Desktop.Values;
using TgPics.Desktop.Views.Pages;
using VkNet.Model;

public interface ISettingsVM
{
    Command LogInTgPicsCommand { get; }
    Command LogInVkCommand { get; }
    Command LogOutOfVkCommand { get; }
    Command<LoginPage> ProceedLoginCommand { get; }
    Command SaveHostCommand { get; }
    Command SavePostingTagCommand { get; }
    FaveTag SelectedPostingTag { get; set; }
    string TgPicsHost { get; set; }
    bool TgPicsIsLoggedIn { get; set; }
    string TgPicsUsername { get; set; }
    bool VkIsLoggedIn { get; set; }
    List<FaveTag> VkTags { get; set; }
    string VkUsername { get; set; }
}

internal class SettingsVM : ViewModelBase, ISettingsVM
{
    public SettingsVM(
        INavigationService navigationService,
        ISettingsService settingsService,
        IVkApiService vkApiService)
    {
        this.navigationService = navigationService;
        this.settingsService = settingsService;
        this.vkApiService = vkApiService;

        SaveHostCommand = new(SaveHost);
        LogInTgPicsCommand = new(LogInTgPics);
        ProceedLoginCommand = new(ProceedLogin);

        LogInVkCommand = new(LogInVk);
        LogOutOfVkCommand = new(vkApiService.LogOut);
        SavePostingTagCommand = new(SavePostingTag);

        TgPicsHost = settingsService.Get<string>(SettingsKeys.TG_PICS_HOST);
        TgPicsUsername = settingsService.Get<string>(SettingsKeys.TG_PICS_USERNAME);

        if (!string.IsNullOrEmpty(settingsService.Get<string>(SettingsKeys.TG_PICS_TOKEN)))
            TgPicsIsLoggedIn = true;

        ConfigureVkSection();
    }

    readonly INavigationService navigationService;
    readonly ISettingsService settingsService;
    readonly IVkApiService vkApiService;


    string tgPicsHost = string.Empty;
    bool tgPicsIsLoggedIn = false;
    string tgPicsUsername = string.Empty;

    bool vkIsLoggedIn = false;
    string vkUsername = string.Empty;
    List<FaveTag> vkTags;
    FaveTag selectedPostingTag;


    public string TgPicsHost
    {
        get => tgPicsHost;
        set => SetProperty(ref tgPicsHost, value);
    }

    public bool TgPicsIsLoggedIn
    {
        get => tgPicsIsLoggedIn;
        set => SetProperty(ref tgPicsIsLoggedIn, value);
    }

    public string TgPicsUsername
    {
        get => tgPicsUsername;
        set => SetProperty(ref tgPicsUsername, value);
    }


    public bool VkIsLoggedIn
    {
        get => vkIsLoggedIn;
        set => SetProperty(ref vkIsLoggedIn, value);
    }

    public string VkUsername
    {
        get => vkUsername;
        set => SetProperty(ref vkUsername, value);
    }

    public List<FaveTag> VkTags
    {
        get => vkTags;
        set => SetProperty(ref vkTags, value);
    }

    public FaveTag SelectedPostingTag
    {
        get => selectedPostingTag;
        set => SetProperty(ref selectedPostingTag, value);
    }


    public Command SaveHostCommand { get; private set; }
    public Command LogInTgPicsCommand { get; private set; }
    public Command<LoginPage> ProceedLoginCommand { get; private set; }

    public Command LogInVkCommand { get; private set; }
    public Command LogOutOfVkCommand { get; private set; }
    public Command SavePostingTagCommand { get; private set; }


    private void SaveHost() =>
        settingsService.Set(SettingsKeys.TG_PICS_HOST, TgPicsHost);

    private async void LogInTgPics()
    {
        var page = new LoginPage();
        var dialog = new ContentDialog()
        {
            Title = "Вход в аккаунт",
            Content = page,
            PrimaryButtonText = "Войти",
            PrimaryButtonCommand = ProceedLoginCommand,
            DefaultButton = ContentDialogButton.Primary,
            CloseButtonText = "Отмена",
            PrimaryButtonCommandParameter = page
        };

        await navigationService.ShowDialogAsync(dialog);
    }

    private async void LogInVk()
    {
        await vkApiService.AuthorizeAsync();
        ConfigureVkSection();
    }

    private async void ProceedLogin(LoginPage page)
    {
        var client = new TgPicsApi(TgPicsHost, secure: false);
        var request = new UsersAuthRequest
        {
            Username = page.ViewModel.Username,
            Password = page.ViewModel.Password
        };

        try
        {
            var response = await client.AuthAsync(request);
            settingsService.Set(SettingsKeys.TG_PICS_TOKEN, response.Token);
            settingsService.Set(SettingsKeys.TG_PICS_USERNAME, response.Username);
            TgPicsIsLoggedIn = true;
            NotifyPropertyChanged(TgPicsUsername);
        }
        catch (Exception ex)
        {
            await navigationService.ShowDialogAsync(new ContentDialog().AsError(ex));
        }
    }

    private void SavePostingTag() =>
        settingsService.Set(SettingsKeys.POSTING_TAG, SelectedPostingTag.Id);

    private async void ConfigureVkSection()
    {
        try
        {
            if (!vkApiService.HasSavedToken)
            {
                VkIsLoggedIn = false;
                return;
            }

            if (!vkApiService.IsAuthorized)
            {
                await vkApiService.AuthorizeFromSavedAsync();
                VkIsLoggedIn = true;
            }

            var info = vkApiService.Api.Account.GetProfileInfo();
            VkUsername = $"{info.FirstName} {info.LastName}";

            VkTags = vkApiService.Api.Fave.GetTags().Select(t => t).ToList();

            SelectedPostingTag = VkTags
                .FirstOrDefault(t => t.Id == settingsService.Get<long>(SettingsKeys.POSTING_TAG));
        }
        catch (Exception ex)
        {
            await navigationService.ShowDialogAsync(new ContentDialog().AsError(ex));
        }
    }
}