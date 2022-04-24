namespace TgPics.Desktop.ViewModels.Pages;

using DesktopKit.Services;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using TgPics.Api.Client;
using TgPics.Core.Models.Requests;
using TgPics.Desktop.Helpers;
using TgPics.Desktop.MVVM;
using TgPics.Desktop.Services;
using TgPics.Desktop.Values;
using TgPics.Desktop.Views.Pages;
using VkNet;
using VkNet.Model;

public interface ISettingsVM
{
    RelayCommand LogInTgPicsCommand { get; }
    RelayCommand LogInVkCommand { get; }
    RelayCommand LogOutOfVkCommand { get; }
    RelayCommand<LoginPage> ProceedLoginCommand { get; }
    RelayCommand SaveHostCommand { get; }
    RelayCommand SavePostingTagCommand { get; }
    FaveTag SelectedPostingTag { get; set; }
    string TgPicsHost { get; set; }
    bool TgPicsIsLoggedIn { get; set; }
    string TgPicsUsername { get; set; }
    bool VkIsLoggedIn { get; set; }
    List<FaveTag> VkTags { get; set; }
    string VkUsername { get; set; }
}

// TODO: вынести часть логики в сервис.
// да даже не один сервис тут нужен.. 
internal class SettingsVM : ViewModelBase, ISettingsVM
{
    public SettingsVM(
        INavigationService navigationService,
        ISettingsService settingsService)
    {
        this.navigationService = navigationService;
        this.settingsService = settingsService;

        SaveHostCommand = new(SaveHost);
        LogInTgPicsCommand = new(LogInTgPics);
        ProceedLoginCommand = new(ProceedLogin);

        LogInVkCommand = new(LogInVk);
        LogOutOfVkCommand = new(LogOutOfVk);
        SavePostingTagCommand = new(SavePostingTag);

        TgPicsHost = settingsService.Get<string>(SettingsKeys.TG_PICS_HOST);
        TgPicsUsername = settingsService.Get<string>(SettingsKeys.TG_PICS_USERNAME);

        if (!string.IsNullOrEmpty(settingsService.Get<string>(SettingsKeys.TG_PICS_TOKEN)))
            TgPicsIsLoggedIn = true;

        var vkToken = settingsService.Get<string>(SettingsKeys.VK_TOKEN);
        if (!string.IsNullOrEmpty(vkToken))
            InitializeVkApi(vkToken);
    }

    readonly INavigationService navigationService;
    readonly ISettingsService settingsService;
    VkApi vkApi;


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


    public RelayCommand SaveHostCommand { get; private set; }
    public RelayCommand LogInTgPicsCommand { get; private set; }
    public RelayCommand<LoginPage> ProceedLoginCommand { get; private set; }

    public RelayCommand LogInVkCommand { get; private set; }
    public RelayCommand LogOutOfVkCommand { get; private set; }
    public RelayCommand SavePostingTagCommand { get; private set; }


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
            await navigationService.ShowDialogAsync(new ContentDialog().MakeException(ex));
        }
    }

    private async void LogInVk()
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
            page.ViewModel.LoginCompleted += () =>
            {
                dialog.Hide();
                settingsService.Set(SettingsKeys.VK_TOKEN, page.ViewModel.Token);
                InitializeVkApi(page.ViewModel.Token);
            };
        };

        await navigationService.ShowDialogAsync(dialog);
    }

    private void LogOutOfVk()
    {
        settingsService.Set(SettingsKeys.VK_TOKEN, string.Empty);
        VkIsLoggedIn = false;
    }

    private void SavePostingTag() =>
        settingsService.Set(SettingsKeys.POSTING_TAG, SelectedPostingTag.Id);

    private async void InitializeVkApi(string token)
    {
        try
        {
            vkApi = new VkApi();
            vkApi.Authorize(new ApiAuthParams
            {
                AccessToken = token,
            });

            var info = vkApi.Account.GetProfileInfo();
            VkUsername = $"{info.FirstName} {info.LastName}";
            VkIsLoggedIn = true;

            VkTags = vkApi.Fave.GetTags().Select(t => t).ToList();

            SelectedPostingTag = VkTags
                .FirstOrDefault(t => t.Id == settingsService.Get<long>(SettingsKeys.POSTING_TAG));
        }
        catch (Exception ex)
        {
            await navigationService.ShowDialogAsync(new ContentDialog().MakeException(ex));
        }
    }
}