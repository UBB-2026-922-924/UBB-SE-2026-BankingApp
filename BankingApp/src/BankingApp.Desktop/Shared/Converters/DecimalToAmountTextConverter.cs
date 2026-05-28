namespace BankingApp.Desktop.Shared.Converters;

using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

/// <summary>Formats decimal amounts with two fraction digits.</summary>
public sealed partial class DecimalToAmountTextConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is decimal amount
            ? amount.ToString("N2", CultureInfo.InvariantCulture)
            : "0.00";
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return decimal.TryParse(value.ToString(), out decimal amount)
            ? amount
            : 0m;
    }
}
