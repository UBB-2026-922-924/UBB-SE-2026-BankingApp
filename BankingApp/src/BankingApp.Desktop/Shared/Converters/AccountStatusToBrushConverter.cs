namespace BankingApp.Desktop.Shared.Converters;

using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

/// <summary>
///     Converts savings account status values to badge brushes.
/// </summary>
public sealed partial class AccountStatusToBrushConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        string status = value?.ToString() ?? string.Empty;

        return new SolidColorBrush(status.Equals("Active", StringComparison.OrdinalIgnoreCase)
            ? ColorHelper.FromArgb(255, 15, 118, 110)
            : ColorHelper.FromArgb(255, 107, 114, 128));
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
