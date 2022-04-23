namespace TgPics.Desktop.Helpers;

using Microsoft.UI.Xaml.Controls;
using System;
using TgPics.Desktop.Views.UserControls;

internal static class ContentDialogPresets
{
    public static ContentDialog MakeException(this ContentDialog dialog, Exception ex)
    {
        dialog.Title = "Ошибка 😢";
        dialog.Content = new ExceptionUserControl(ex);
        dialog.DefaultButton = ContentDialogButton.Close;
        dialog.CloseButtonText = "ок бро";

        return dialog;
    }

    public static ContentDialog MakeSuccessful(this ContentDialog dialog, string message)
    {
        dialog.Title = "Успех 🤩";
        dialog.Content = message;
        dialog.DefaultButton = ContentDialogButton.Close;
        dialog.CloseButtonText = "крутяк!!";

        return dialog;
    }
}
