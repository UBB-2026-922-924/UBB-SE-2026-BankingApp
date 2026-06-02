namespace BankingApp.Desktop.Views;

using Contracts.Features.Chat.Dtos;
using Infrastructure.Http.Features.Chat.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

/// <summary>
///     Displays a single support chat conversation.
/// </summary>
public sealed partial class ChatView : Page
{
    private readonly IChatRepoProxy _chatService;
    private readonly DispatcherTimer _refreshTimer;
    private int _sessionId;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ChatView"/> class.
    /// </summary>
    public ChatView(IChatRepoProxy chatService)
    {
        InitializeComponent();
        _chatService = chatService;
        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        _refreshTimer.Tick += RefreshTimer_Tick;
    }

    /// <inheritdoc />
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is int parameterSessionId)
        {
            _sessionId = parameterSessionId;
        }

        if (_sessionId <= 0)
        {
            return;
        }

        try
        {
            ChatSessionDto session = await _chatService.GetSessionAsync(_sessionId);
            HeaderText.Text = $"{session.Subject} - #{session.Id}";

            await LoadMessagesAsync();
            _refreshTimer.Start();
        }
        catch (Exception ex)
        {
            HeaderText.Text = $"Chat #{_sessionId}";
            var dialog = new ContentDialog
            {
                Title = "Could not load chat",
                Content = ex.Message,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }
    }

    /// <inheritdoc />
    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        _refreshTimer.Stop();
    }

    private async Task LoadMessagesAsync()
    {
        ChatSessionDto session = await _chatService.GetSessionAsync(_sessionId);
        List<ChatMessageDto> messages = session.Messages;
        MessagesList.ItemsSource = messages;
        if (messages.Count > 0)
        {
            MessagesList.ScrollIntoView(messages.Last());
        }
    }

    private async void AskPresetQuestionButton_Click(object sender, RoutedEventArgs e)
    {
        string content = PresetQuestionsComboBox.SelectedItem?.ToString()?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(content) || _sessionId <= 0)
        {
            return;
        }

        await _chatService.CreateMessageAsync(_sessionId, content);
        PresetQuestionsComboBox.SelectedItem = null;
        AttachmentText.Text = string.Empty;
        await LoadMessagesAsync();
    }

    private void AttachButton_Click(object sender, RoutedEventArgs e)
    {
        AttachmentText.Text = "Attachments are not available for this chat endpoint.";
    }

    private async void EscalateButton_Click(object sender, RoutedEventArgs e)
    {
        await _chatService.CreateMessageAsync(_sessionId, "Please escalate this conversation to a support specialist.");
        await LoadMessagesAsync();
    }

    private async void EndSessionButton_Click(object sender, RoutedEventArgs e)
    {
        await _chatService.CloseSessionAsync(_sessionId);

        var ratingBox = new ComboBox
        {
            PlaceholderText = "Select rating (1-5)"
        };
        ratingBox.Items.Add(1);
        ratingBox.Items.Add(2);
        ratingBox.Items.Add(3);
        ratingBox.Items.Add(4);
        ratingBox.Items.Add(5);

        var feedbackBox = new TextBox
        {
            PlaceholderText = "Optional written feedback."
        };

        var dialog = new ContentDialog
        {
            Title = "Rate your support experience",
            Content = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    ratingBox,
                    feedbackBox
                }
            },
            PrimaryButtonText = "Submit",
            CloseButtonText = "Skip",
            XamlRoot = XamlRoot
        };

        ContentDialogResult result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && ratingBox.SelectedItem is int rating)
        {
            await _chatService.SaveFeedbackAsync(_sessionId, rating, feedbackBox.Text);
        }
    }

    private async void EmailTranscriptButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Email chat transcript",
            Content = "Transcript email is not available for this chat endpoint.",
            CloseButtonText = "OK",
            XamlRoot = XamlRoot
        };
        await dialog.ShowAsync();
    }

    private async void RefreshTimer_Tick(object? sender, object e)
    {
        await LoadMessagesAsync();
    }
}
