namespace BankingApp.Desktop.Shared.Converters;

using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

/// <summary>Formats nullable dates for beneficiary and transfer displays.</summary>
public sealed partial class NullableDateToTextConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is DateTime dateTime
            ? dateTime.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)
            : "—";
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is string text && DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime)
            ? dateTime
            : null!;
    }
}
