namespace BankingApp.Desktop.Shared.Converters;

using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

public sealed partial class DecimalToGainLossBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        decimal amount = value is decimal decimalValue ? decimalValue : 0m;
        return new SolidColorBrush(amount >= 0
            ? ColorHelper.FromArgb(255, 15, 118, 110)
            : ColorHelper.FromArgb(255, 185, 28, 28));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
