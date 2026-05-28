namespace BankingApp.Desktop.Shared.Converters;

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

/// <summary>Maps <see langword="false"/> to <see cref="Visibility.Visible"/> and <see langword="true"/> to <see cref="Visibility.Collapsed"/>.</summary>
public partial class InverseBoolToVisibilityConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is true ? Visibility.Collapsed : Visibility.Visible;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is not Visibility.Visible;
    }
}
