namespace BankingApp.Desktop.Shared.Converters;

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

/// <summary>
///     Converts the current wizard step number to <see cref="Visibility" />.
///     Returns <see cref="Visibility.Visible" /> when the bound step matches the
///     target step number supplied as a string converter parameter; collapses otherwise.
/// </summary>
public partial class StepToVisibilityConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int currentStepNumber
            && parameter is string targetStepText
            && int.TryParse(targetStepText, out int targetStepNumber))
        {
            return currentStepNumber == targetStepNumber ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
