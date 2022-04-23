﻿namespace TgPics.Desktop.ViewModels.Pages;

using DesktopKit.Services;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TgPics.Api.Client;
using TgPics.Core.Models.Requests;
using TgPics.Desktop.Helpers;
using TgPics.Desktop.MVVM;
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
public class SettingsVM : ViewModelBase, ISettingsVM
{
    public SettingsVM(INavigationService navigationService)
    {
        this.navigationService = navigationService;

        SaveHostCommand = new(SaveHost);
        LogInTgPicsCommand = new(LogInTgPics);
        ProceedLoginCommand = new(ProceedLogin);

        LogInVkCommand = new(LogInVk);
        LogOutOfVkCommand = new(LogOutOfVk);
        SavePostingTagCommand = new(SavePostingTag);

        TgPicsHost = settings.Get<string>(TG_PICS_HOST);
        TgPicsUsername = settings.Get<string>(TG_PICS_USERNAME);

        if (!string.IsNullOrEmpty(settings.Get<string>(TG_PICS_TOKEN)))
            TgPicsIsLoggedIn = true;

        var vkToken = settings.Get<string>(VK_TOKEN);
        if (!string.IsNullOrEmpty(vkToken))
            InitializeVkApi(vkToken);
    }


    public const string TG_PICS_HOST = "tg_pics_host";
    public const string TG_PICS_TOKEN = "tg_pics_token";
    public const string TG_PICS_USERNAME = "tg_pics_username";

    public const string VK_TOKEN = "vk_token";
    public const string POSTING_TAG = "posting_tag";


    readonly INavigationService navigationService;
    readonly Settings settings = Settings.Instance;
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
        settings.Set(TG_PICS_HOST, TgPicsHost);

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
            settings.Set(TG_PICS_TOKEN, response.Token);
            settings.Set(TG_PICS_USERNAME, response.Username);
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
                settings.Set(VK_TOKEN, page.ViewModel.Token);
                InitializeVkApi(page.ViewModel.Token);
            };
        };

        await navigationService.ShowDialogAsync(dialog);
    }

    private void LogOutOfVk()
    {
        settings.Set(VK_TOKEN, string.Empty);
        VkIsLoggedIn = false;
    }

    private void SavePostingTag() =>
        settings.Set(POSTING_TAG, SelectedPostingTag.Id);

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
                .FirstOrDefault(t => t.Id == settings.Get<long>(POSTING_TAG));
        }
        catch (Exception ex)
        {
            await navigationService.ShowDialogAsync(new ContentDialog().MakeException(ex));
        }
    }
}