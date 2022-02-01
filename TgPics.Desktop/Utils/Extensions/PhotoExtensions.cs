namespace TgPics.Desktop.Utils.Extensions;

using System;
using System.Linq;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.Attachments;

public static class PhotoExtensions
{
    public static Uri GetLargestImageUri(this Photo photo)
        => photo.Sizes
        .OrderByDescending(s => s.Type.ToInt())
        .FirstOrDefault()
        .Url;

    public static Uri GetSmallestImageUri(this Photo photo)
        => photo.Sizes
        .OrderBy(s => s.Type.ToInt())
        .FirstOrDefault()
        .Url;

    /// <summary>
    /// Возвращает наибольшее доступное изображение с соотношением
    /// сторон 3:2.
    /// </summary>
    public static Uri GetLargest32AspectRatioImageUri(this Photo photo)
        => photo.Sizes
        .Where(s => Is32AcpectRatio(s.Type))
        .OrderByDescending(s => s.Type.ToInt())
        .FirstOrDefault()
        .Url;

    /// <summary>
    /// Возвращает наименьшее доступное изображение с соотношением
    /// сторон 3:2.
    /// </summary>
    public static Uri GetSmallest32AspectRatioImageUri(this Photo photo)
       => photo.Sizes
       .Where(s => Is32AcpectRatio(s.Type))
       .OrderBy(s => s.Type.ToInt())
       .FirstOrDefault()
       .Url;

    private static bool Is32AcpectRatio(PhotoSizeType type) =>
        type == PhotoSizeType.O ||
        type == PhotoSizeType.P ||
        type == PhotoSizeType.Q ||
        type == PhotoSizeType.R;

    public static int ToInt(this PhotoSizeType type)
    {
        if (type == PhotoSizeType.S)
            return 0;
        else if (type == PhotoSizeType.M)
            return 1;
        else if (type == PhotoSizeType.X)
            return 2;
        else if (type == PhotoSizeType.O)
            return 3;
        else if (type == PhotoSizeType.P)
            return 4;
        else if (type == PhotoSizeType.Q)
            return 5;
        else if (type == PhotoSizeType.R)
            return 6;
        else if (type == PhotoSizeType.Y)
            return 7;
        else if (type == PhotoSizeType.Z)
            return 8;
        else if (type == PhotoSizeType.W)
            return 9;
        else
            return -1;
    }
}