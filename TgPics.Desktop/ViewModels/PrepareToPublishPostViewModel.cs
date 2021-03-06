namespace TgPics.Desktop.ViewModels;

using DesktopKit.MVVM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TgPics.Core.Models;
using TgPics.Desktop.Services;
using TgPics.Desktop.Values;
using VkNet;
using VkNet.Model.RequestParams.Fave;

internal class PrepareToPublishPostViewModel : IModel<PrepareToPublishPost>
{
    public PrepareToPublishPostViewModel(
        ISettingsService settingsService, IVkApiService vkApiService, PrepareToPublishPost post)
    {
        this.settingsService = settingsService;
        this.vkApiService = vkApiService;

        Model = post;
        Photos = post.Photos.Select(
            p => new PrepareToPublishPhotoViewModel(p)).ToList();
    }

    readonly ISettingsService settingsService;
    readonly IVkApiService vkApiService;

    public PrepareToPublishPost Model { get; set; }
    public List<PrepareToPublishPhotoViewModel> Photos { get; set; }

    // TODO: переделать как надо, чтобы потом масштабировать эту хуйню
    public void SetTagToOriginalPost()
    {
        var ownerId = Convert.ToInt64(
            Regex.Match(Model.SourceLink.ToString(), @"(?<=wall)(.*)(?=_)").Value);

        var postId = Convert.ToInt64(
            Regex.Match(Model.SourceLink.ToString(), @"(?<=_)(.*)").Value);

        var tagId = settingsService.Get<long>(SettingsKeys.POSTING_TAG);

        vkApiService.Api.Fave.SetTags(new FaveSetTagsParams
        {
            ItemType = VkNet.Enums.Filters.FaveType.Post,
            ItemOwnerId = ownerId,
            ItemId = postId,
            TagIds = new List<long> { tagId }
        });
    }
}