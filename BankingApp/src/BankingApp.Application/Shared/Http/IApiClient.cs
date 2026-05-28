namespace BankingApp.Application.Shared.Http;

using ErrorOr;

/// <summary>
///     Defines the low-level HTTP boundary used by client-side application code.
/// </summary>
public interface IApiClient
{
    /// <summary>
    ///     Gets the currently configured bearer token, if any.
    /// </summary>
    public string? Token { get; }

    /// <summary>
    ///     Returns a success result when the client is correctly configured.
    /// </summary>
    public ErrorOr<Success> EnsureConfigured();

    /// <summary>
    ///     Sets the bearer token used for authenticated requests.
    /// </summary>
    public void SetToken(string token);

    /// <summary>
    ///     Clears any stored authentication state.
    /// </summary>
    public void ClearToken();

    public Task<ErrorOr<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default);

    public Task<ErrorOr<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest? data, CancellationToken cancellationToken = default);

    public Task<ErrorOr<Success>> PostAsync<TRequest>(string endpoint, TRequest? data, CancellationToken cancellationToken = default);

    public Task<ErrorOr<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest? data, CancellationToken cancellationToken = default);

    public Task<ErrorOr<Success>> PutAsync<TRequest>(string endpoint, TRequest? data, CancellationToken cancellationToken = default);

    public Task<ErrorOr<Success>> DeleteAsync(string endpoint, CancellationToken cancellationToken = default);
}
