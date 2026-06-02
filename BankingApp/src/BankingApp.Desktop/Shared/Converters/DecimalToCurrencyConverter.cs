namespace BankingApp.Desktop.Shared.Converters;

using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

/// <summary>
///     Formats decimal values as currency labels.
/// </summary>
public sealed partial class DecimalToCurrencyConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            decimal amount => amount.ToString("C2", CultureInfo.CurrentCulture),
            double amount => amount.ToString("C2", CultureInfo.CurrentCulture),
            _ => string.Empty
        };
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
