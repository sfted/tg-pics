namespace TgPics.Desktop.ViewModels.Pages;

using DesktopKit.MVVM;
using DesktopKit.Services;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TgPics.Desktop.Helpers;
using TgPics.Desktop.Services;
using TgPics.Desktop.Values;
using VkNet.Model;

public interface ISettingsVM
{
    Command LogInTgPicsCommand { get; }
    Command LogOutOfTgPicsCommand { get; }

    Command LogInVkCommand { get; }
    Command LogOutOfVkCommand { get; }
    Command SavePostingTagCommand { get; }

    bool TgPicsIsLoggedIn { get; set; }
    string TgPicsUsername { get; set; }

    bool VkIsLoggedIn { get; set; }
    string VkUsername { get; set; }
    List<FaveTag> VkTags { get; set; }
    FaveTag SelectedPostingTag { get; set; }
}

internal class SettingsVM : ViewModelBase, ISettingsVM
{
    public SettingsVM(
        INavigationService navigationService,
        ISettingsService settingsService,
        ITgPicsService tgPicsService,
        IVkApiService vkApiService)
    {
        this.navigationService = navigationService;
        this.settingsService = settingsService;
        this.tgPicsService = tgPicsService;
        this.vkApiService = vkApiService;

        LogInTgPicsCommand = new(LogInTgPics);
        LogOutOfTgPicsCommand = new(tgPicsService.LogOut);

        LogInVkCommand = new(LogInVk);
        LogOutOfVkCommand = new(vkApiService.LogOut);
        SavePostingTagCommand = new(SavePostingTag);

        ConfigureSections();
    }

    readonly INavigationService navigationService;
    readonly ISettingsService settingsService;
    readonly ITgPicsService tgPicsService;
    readonly IVkApiService vkApiService;


    bool tgPicsIsLoggedIn = false;
    string tgPicsUsername = string.Empty;

    bool vkIsLoggedIn = false;
    string vkUsername = string.Empty;
    List<FaveTag> vkTags;
    FaveTag selectedPostingTag;


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


    public Command LogInTgPicsCommand { get; private set; }
    public Command LogOutOfTgPicsCommand { get; private set; }

    public Command LogInVkCommand { get; private set; }
    public Command LogOutOfVkCommand { get; private set; }
    public Command SavePostingTagCommand { get; private set; }


    private async void LogInTgPics()
    {
        await tgPicsService.AuthorizeAsync();
        await ConfigureTgPicsSection();
    }

    private async void LogInVk()
    {
        await vkApiService.AuthorizeAsync();
        await ConfigureVkSection();
    }

    private void SavePostingTag() =>
        settingsService.Set(SettingsKeys.POSTING_TAG, SelectedPostingTag.Id);

    private async void ConfigureSections()
    {
        await ConfigureTgPicsSection();
        await ConfigureVkSection();
    }

    private async Task ConfigureTgPicsSection()
    {
        try
        {
            if (!tgPicsService.HasSavedToken)
            {
                TgPicsIsLoggedIn = false;
                return;
            }

            if (!tgPicsService.IsAuthorized)
            {
                tgPicsService.AuthorizeFromSaved();
                TgPicsIsLoggedIn = true;
            }

            TgPicsUsername = settingsService.Get<string>(SettingsKeys.TG_PICS_USERNAME);
            TgPicsIsLoggedIn = true;
        }
        catch (Exception ex)
        {
            await navigationService.ShowDialogAsync(new ContentDialog().AsError(ex));
        }
    }

    private async Task ConfigureVkSection()
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

            VkIsLoggedIn = true;
        }
        catch (Exception ex)
        {
            await navigationService.ShowDialogAsync(new ContentDialog().AsError(ex));
        }
    }
}