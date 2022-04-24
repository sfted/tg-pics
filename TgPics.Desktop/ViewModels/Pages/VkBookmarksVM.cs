namespace TgPics.Desktop.ViewModels.Pages;

using DesktopKit.MVVM;
using DesktopKit.Services;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TgPics.Desktop.Helpers;
using TgPics.Desktop.Services;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Utils;

internal interface IVkBookmarksVM
{
    ObservableCollection<FaveGetObjectVM> Items { get; }
    AsyncRelayCommand LoadMoreCommand { get; }
}

internal class VkBookmarksVM : ViewModelBase, IVkBookmarksVM
{
    public VkBookmarksVM(
        INavigationService navigationService,
        ISettingsService settingsService,
        IVkApiService vkApiService)
    {
        this.navigationService = navigationService;
        this.settingsService = settingsService;
        this.vkApiService = vkApiService;

        // TODO: сделать автоподгрузку
        LoadMoreCommand = new(LoadItems);
        InitVkApi();
    }

    readonly INavigationService navigationService;
    readonly ISettingsService settingsService;
    readonly IVkApiService vkApiService;
    int offset = 0;

    public ObservableCollection<FaveGetObjectVM> Items { get; private set; } = new();

    public AsyncRelayCommand LoadMoreCommand { get; private set; }

    private async void InitVkApi()
    {
        try
        {
            if (vkApiService.IsAuthorized)
                return;

            if (vkApiService.HasSavedToken)
                await vkApiService.AuthorizeFromSavedAsync();
            else
            {
                await vkApiService.AuthorizeAsync();
                InitVkApi();
            }
        }
        catch (Exception ex)
        {
            await navigationService.ShowDialogAsync(new ContentDialog().AsError(ex));
        }
    }


    private async Task LoadItems()
    {
        try
        {
            var parameters = new VkParameters
            {
                { "count", 50 },
                { "extended", 1 },
                { "offset", offset },
                { "item_type", "post" },
                { "access_token", vkApiService.Api.Token },
                { "v", VkLoginPageVM.API_VERSION }
            };

            var scheme = new
            {
                Response = new
                {
                    Count = 0,
                    Items = new List<FaveGetObjectButBetter>(),
                    Profiles = new List<User>(),
                    Groups = new List<Group>(),
                },
            };

            var json = await vkApiService.Api.InvokeAsync("fave.get", parameters);

            var queue = DispatcherQueue.GetForCurrentThread();

            await Task.Run(() =>
            {
                var response = JsonConvert.DeserializeAnonymousType(json, scheme);

                foreach (var item in response.Response.Items)
                {
                    var vm = new FaveGetObjectVM(
                        navigationService,
                        settingsService,
                        vkApiService,
                        item,
                        response.Response.Profiles,
                        response.Response.Groups);

                    queue.TryEnqueue(() =>
                    {
                        Items.Add(vm);
                    });
                }

                offset += response.Response.Items.Count;
            });
        }
        catch (Exception ex)
        {
            await navigationService.ShowDialogAsync(new ContentDialog().AsError(ex));
        }
    }

    public class FaveGetObjectButBetter : FaveGetObject
    {
        [JsonProperty("post")]
        public new PostButBetter Post { get; set; }
    }

    public class PostButBetter : Post
    {
        [JsonProperty("signer_id")]
        public long? SignerIdSignerIdButNotAFuckingNull { get; set; }

        [JsonProperty("date")]
        public double DateSeconds { get; set; }
    }
}