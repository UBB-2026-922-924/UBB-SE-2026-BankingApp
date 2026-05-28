namespace BankingApp.Desktop.Views;

using System;
using System.Collections.Generic;
using Contracts.Features.UserProfile.Dtos;
using Domain.Common.Extensions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Serilog;

public sealed partial class ProfileView
{
    private void PopulateNotificationPreferences(List<NotificationPreferenceDto>? preferences)
    {
        Log.Information(
            "ProfileView populating notification preferences. Count={Count}.",
            preferences?.Count ?? 0);
        _viewModel.IsInitializingView = true;
        NotificationPreferencesPanel.Children.Clear();
        if (preferences == null)
        {
            _viewModel.IsInitializingView = false;
            return;
        }

        foreach (NotificationPreferenceDto preference in preferences)
        {
            Grid row = BuildNotificationRow(preference);
            NotificationPreferencesPanel.Children.Add(row);
        }

        _viewModel.IsInitializingView = false;
    }

    private Grid BuildNotificationRow(NotificationPreferenceDto preference)
    {
        var row = new Grid
        {
            Margin = new Thickness(0, 6, 0, 6)
        };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var text = new TextBlock
        {
            Text = preference.Category.ToDisplayName(),
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 13,
            Foreground = GetPrimaryTextBrush()
        };
        var toggle = new ToggleSwitch
        {
            IsOn = preference.EmailEnabled,
            Tag = preference,
            VerticalAlignment = VerticalAlignment.Center
        };
        toggle.Toggled += NotificationToggle_Toggled;

        Grid.SetColumn(text, 0);
        Grid.SetColumn(toggle, 1);
        row.Children.Add(text);
        row.Children.Add(toggle);
        return row;
    }

    private async void NotificationToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (_viewModel.IsInitializingView)
        {
            return;
        }

        if (sender is ToggleSwitch { Tag: NotificationPreferenceDto preference } toggle)
        {
            try
            {
                _isUpdatingToggle = true;
                await _viewModel.ToggleNotificationPreference(preference, toggle.IsOn);
                _isUpdatingToggle = false;
                _viewModel.IsInitializingView = true;
                toggle.IsOn = preference.EmailEnabled;
                _viewModel.IsInitializingView = false;
            }
            catch (Exception ex)
            {
                _isUpdatingToggle = false;
                _viewModel.IsInitializingView = false;
                ShowError(ex.Message);
            }
        }
    }
}
