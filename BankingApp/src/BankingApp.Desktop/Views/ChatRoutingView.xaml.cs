namespace BankingApp.Desktop.Views;

using Contracts.Features.Chat.Dtos;
using Infrastructure.Http.Features.Chat.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

/// <summary>
///     Displays support chat sessions and starts new chats.
/// </summary>
public sealed partial class ChatRoutingView : Page
{
    private readonly IChatRepoProxy _chatService;

    /// <summary>
    ///     Initializes a new instance of the chat routing view.
    /// </summary>
    public ChatRoutingView()
    {
        InitializeComponent();
        _chatService = ((App)Application.Current).Services.GetRequiredService<IChatRepoProxy>();
        Loaded += ChatRoutingView_Loaded;
    }

    private async void ChatRoutingView_Loaded(object sender, RoutedEventArgs e)
    {
        List<ChatSessionDto> sessions = await _chatService.GetSessionsAsync();
        WaitTimeText.Text = $"Estimated wait time: {Math.Max(1, sessions.Count + 1)} minute(s)";
        SessionsList.ItemsSource = sessions;
    }

    private async void StartNewChat_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string issueCategory = IssueCategoryComboBox.SelectedItem?.ToString() ?? "General";
            ChatSessionDto response = await _chatService.CreateSessionAsync(issueCategory);
            if (response.Id <= 0)
            {
                return;
            }

            Frame?.Navigate(typeof(ChatView), response.Id);
        }
        catch (Exception ex)
        {
            var dialog = new ContentDialog
            {
                Title = "Could not start chat",
                Content = ex.Message,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }
    }

    private void SessionsList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is ChatSessionDto session)
        {
            Frame?.Navigate(typeof(ChatView), session.Id);
        }
    }
}
