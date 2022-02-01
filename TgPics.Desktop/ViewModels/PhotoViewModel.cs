namespace TgPics.Desktop.ViewModels;

using System;
using TgPics.Desktop.MVVM.Interfaces;
using TgPics.Desktop.Utils.Extensions;
using VkNet.Model.Attachments;

public class PhotoViewModel : IModel<Photo>
{
    public PhotoViewModel(Photo photo)
    {
        Model = photo;
        OriginalUri = photo.GetLargestImageUri();
        AspectRatio32lUri = photo.GetLargest32AspectRatioImageUri();
    }

    public Photo Model { get; set; }

    public Uri OriginalUri { get; private set; }
    public Uri AspectRatio32lUri { get; private set; }
}