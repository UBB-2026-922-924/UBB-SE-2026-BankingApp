namespace BankingApp.Desktop.Shared.Converters;

using System;
using Microsoft.UI.Xaml.Data;

public sealed partial class DecimalToTrendSymbolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        decimal amount = value is decimal decimalValue ? decimalValue : 0m;
        return amount >= 0 ? "+" : "-";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
