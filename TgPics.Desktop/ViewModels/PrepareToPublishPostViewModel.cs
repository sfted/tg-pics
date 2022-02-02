namespace TgPics.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TgPics.Core.Models;
using TgPics.Desktop.Helpers;
using TgPics.Desktop.MVVM.Interfaces;
using TgPics.Desktop.ViewModels.Pages;
using VkNet;
using VkNet.Model.RequestParams.Fave;

public class PrepareToPublishPostViewModel : IModel<PrepareToPublishPost>
{
    public PrepareToPublishPostViewModel(VkApi api, PrepareToPublishPost post)
    {
        this.api = api;
        Model = post;
        Photos = post.Photos.Select(
            p => new PrepareToPublishPhotoViewModel(p)).ToList();
    }

    private readonly VkApi api;

    public PrepareToPublishPost Model { get; set; }
    public List<PrepareToPublishPhotoViewModel> Photos { get; set; }

    // TODO: переделать как надо, чтобы потом масштабировать эту хуйню
    public void SetTagToOriginalPost()
    {
        var ownerId = Convert.ToInt64(
            Regex.Match(Model.SourceLink.ToString(), @"(?<=wall)(.*)(?=_)").Value);

        var postId = Convert.ToInt64(
            Regex.Match(Model.SourceLink.ToString(), @"(?<=_)(.*)").Value);

        var tagId = Settings.Instance.Get<long>(SettingsViewModel.POSTING_TAG);

        api.Fave.SetTags(new FaveSetTagsParams
        {
            ItemType = VkNet.Enums.Filters.FaveType.Post,
            ItemOwnerId = ownerId,
            ItemId = postId,
            TagIds = new List<long> { tagId }
        });
    }
}