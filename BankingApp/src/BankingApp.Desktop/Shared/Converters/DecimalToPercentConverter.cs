namespace BankingApp.Desktop.Shared.Converters;

using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

/// <summary>
///     Formats decimal values as percentage labels.
/// </summary>
public sealed partial class DecimalToPercentConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            decimal amount => amount.ToString("P2", CultureInfo.CurrentCulture),
            double amount => amount.ToString("P2", CultureInfo.CurrentCulture),
            _ => string.Empty
        };
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
