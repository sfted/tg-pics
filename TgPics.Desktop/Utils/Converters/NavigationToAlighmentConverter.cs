namespace TgPics.Desktop.Utils.Converters;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

internal class NavigationToAlighmentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        ((bool)value) ? HorizontalAlignment.Left : HorizontalAlignment.Center;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}
