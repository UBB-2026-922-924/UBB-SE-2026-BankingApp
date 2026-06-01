namespace BankingApp.Desktop.Shared.Converters;

using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

public sealed partial class DateToMonthYearConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            DateTime date => date.ToString("MMM yyyy", CultureInfo.CurrentCulture),
            DateTimeOffset date => date.ToString("MMM yyyy", CultureInfo.CurrentCulture),
            _ => string.Empty
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
