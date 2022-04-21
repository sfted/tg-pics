namespace TgPics.Desktop.Utils.Converters;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

internal class NavigationToMarginConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        ((bool)value) ? new Thickness(250, 2, 0, 0) : new Thickness(120, 2, 0, 0);

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}
