namespace BankingApp.Desktop.Views;

using System;
using System.Threading.Tasks;
using Contracts.Features.UserProfile.Dtos;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

public sealed partial class ProfileView
{
    private static readonly Windows.UI.Color _sessionBorderColor =
        ColorHelper.FromArgb(255, 226, 232, 240);

    private static readonly Windows.UI.Color _sessionPrimaryTextColor =
        ColorHelper.FromArgb(255, 30, 41, 59);

    private static readonly Windows.UI.Color _sessionSecondaryTextColor =
        ColorHelper.FromArgb(255, 100, 116, 139);

    private static readonly Windows.UI.Color _sessionMutedTextColor =
        ColorHelper.FromArgb(255, 148, 163, 184);

    private async Task LoadSessionsAsync()
    {
        SessionsErrorBar.IsOpen = false;
        SessionsSuccessBar.IsOpen = false;
        SessionsListPanel.Children.Clear();
        NoSessionsText.Visibility = Visibility.Collapsed;
        (bool loaded, string? errorMessage) = await _viewModel.LoadSessionsForCurrentUser();
        if (!loaded)
        {
            SessionsErrorBar.Message = errorMessage ?? "Failed to load active sessions.";
            SessionsErrorBar.IsOpen = true;
            return;
        }

        RenderSessions();
    }

    private void RenderSessions()
    {
        SessionsListPanel.Children.Clear();
        NoSessionsText.Visibility = Visibility.Collapsed;
        if (_viewModel.Sessions.ActiveSessions.Count == 0)
        {
            NoSessionsText.Visibility = Visibility.Visible;
            return;
        }

        foreach (SessionDto session in _viewModel.Sessions.ActiveSessions)
        {
            SessionsListPanel.Children.Add(BuildSessionCard(session));
        }
    }

    private Border BuildSessionCard(SessionDto session)
    {
        var infoStack = new StackPanel { Spacing = 2 };
        infoStack.Children.Add(new TextBlock
        {
            Text = session.DeviceInfo ?? "Unknown Device",
            FontSize = 13,
            Foreground = new SolidColorBrush(_sessionPrimaryTextColor)
        });
        infoStack.Children.Add(new TextBlock
        {
            Text = session.Browser ?? "Unknown Browser",
            FontSize = 12,
            Foreground = new SolidColorBrush(_sessionSecondaryTextColor)
        });
        infoStack.Children.Add(new TextBlock
        {
            Text = $"IP: {session.IpAddress ?? "Unknown"}",
            FontSize = 12,
            Foreground = new SolidColorBrush(_sessionSecondaryTextColor)
        });
        infoStack.Children.Add(new TextBlock
        {
            Text = session.LastActiveAt.HasValue
                ? $"Last active: {session.LastActiveAt.Value:g}"
                : "Last active: Unknown",
            FontSize = 11,
            Foreground = new SolidColorBrush(_sessionMutedTextColor)
        });

        var revokeButton = new Button
        {
            Content = "Revoke",
            Tag = session.Id,
            VerticalAlignment = VerticalAlignment.Center,
            Style = (Style)Resources["DangerButtonStyle"]
        };
        revokeButton.Click += RevokeSessionButton_Click;

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Grid.SetColumn(infoStack, 0);
        Grid.SetColumn(revokeButton, 1);
        grid.Children.Add(infoStack);
        grid.Children.Add(revokeButton);

        return new Border
        {
            Background = new SolidColorBrush(Colors.White),
            CornerRadius = new CornerRadius(10),
            BorderBrush = new SolidColorBrush(_sessionBorderColor),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(16, 12, 16, 12),
            Child = grid
        };
    }

    private async void RevokeSessionButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: int sessionId })
        {
            return;
        }

        try
        {
            (bool success, string? errorMessage) = await _viewModel.RevokeSessionAndReload(sessionId);
            if (success)
            {
                RenderSessions();
                SessionsSuccessBar.Message = "Session revoked successfully.";
                SessionsSuccessBar.IsOpen = true;
            }
            else
            {
                SessionsErrorBar.Message = errorMessage ?? "Failed to revoke session.";
                SessionsErrorBar.IsOpen = true;
            }
        }
        catch (Exception ex)
        {
            SessionsErrorBar.Message = ex.Message;
            SessionsErrorBar.IsOpen = true;
        }
    }
}
