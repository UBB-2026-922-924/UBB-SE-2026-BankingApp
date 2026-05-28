namespace BankingApp.Desktop.Shared.Converters;

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

/// <summary>Maps <see langword="true"/> to <see cref="Visibility.Visible"/> and <see langword="false"/> to <see cref="Visibility.Collapsed"/>.</summary>
public partial class BoolToVisibilityConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is true ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is Visibility.Visible;
    }
}
