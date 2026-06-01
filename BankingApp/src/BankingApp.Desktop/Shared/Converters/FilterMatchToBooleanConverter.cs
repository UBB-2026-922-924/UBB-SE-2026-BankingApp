namespace BankingApp.Desktop.Shared.Converters;

using System;
using Microsoft.UI.Xaml.Data;

public sealed partial class FilterMatchToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return string.Equals(value?.ToString(), parameter?.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is true ? parameter?.ToString() ?? string.Empty : string.Empty;
    }
}
