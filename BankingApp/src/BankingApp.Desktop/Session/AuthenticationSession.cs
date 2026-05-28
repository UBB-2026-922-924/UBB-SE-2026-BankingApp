namespace BankingApp.Desktop.Session;

using Application.Shared.Http;
using ErrorOr;

/// <summary>Default desktop authentication session backed by the configured API client.</summary>
public sealed class AuthenticationSession(IApiClient apiClient) : IAuthenticationSession
{
    /// <inheritdoc />
    public int? CurrentUserId { get; set; }

    /// <inheritdoc />
    public string? Token => apiClient.Token;

    /// <inheritdoc />
    public ErrorOr<Success> EnsureConfigured() => apiClient.EnsureConfigured();

    /// <inheritdoc />
    public void SetToken(string token) => apiClient.SetToken(token);

    /// <inheritdoc />
    public void Clear()
    {
        CurrentUserId = null;
        apiClient.ClearToken();
    }
}
