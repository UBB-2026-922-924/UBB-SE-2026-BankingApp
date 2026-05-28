namespace BankingApp.Desktop.Session;

using ErrorOr;

/// <summary>Stores desktop authentication state and applies bearer tokens for API calls.</summary>
public interface IAuthenticationSession
{
    /// <summary>Gets or sets the current authenticated user identifier.</summary>
    public int? CurrentUserId { get; set; }

    /// <summary>Gets the bearer token currently available to desktop API clients.</summary>
    public string? Token { get; }

    /// <summary>Checks whether the underlying API client has a usable configuration.</summary>
    public ErrorOr<Success> EnsureConfigured();

    /// <summary>Stores the bearer token used for authenticated requests.</summary>
    public void SetToken(string token);

    /// <summary>Clears any authenticated desktop state.</summary>
    public void Clear();
}
