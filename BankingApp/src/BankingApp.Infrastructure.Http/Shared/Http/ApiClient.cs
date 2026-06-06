namespace BankingApp.Infrastructure.Http.Shared.Http;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BankingApp.Application.Shared.Http;
using BankingApp.Contracts.Http;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using InfrastructureHttpLogMessages = Logging.InfrastructureHttpLogMessages;

/// <summary>
///     Provides a thin HTTP client abstraction for desktop and other client-side code.
/// </summary>
public sealed class ApiClient : IApiClient, IDisposable
{
    private readonly Error? _configurationError;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;
    private bool _disposed;

    public ApiClient(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ApiClient> logger)
        : this(httpClientFactory.CreateClient(HttpClientNames.Api), configuration, logger)
    {
    }

    public ApiClient(IConfiguration configuration, ILogger<ApiClient> logger)
        : this(CreateStandaloneClient(configuration), configuration, logger)
    {
    }

    private ApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<ApiClient> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        string? baseUrl = configuration["ApiBaseUrl"];
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            return;
        }

        _configurationError = Error.Failure("ApiClient.MissingBaseUrl", "ApiBaseUrl is missing from configuration.");
        InfrastructureHttpLogMessages.ApiBaseUrlMissing(_logger);
    }

    public string? Token { get; private set; }

    public ErrorOr<Success> EnsureConfigured()
    {
        if (_configurationError is null)
        {
            return Result.Success;
        }

        return _configurationError.Value;
    }

    public void SetToken(string token)
    {
        Token = token;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthHeaderNames.BearerScheme, token);
        InfrastructureHttpLogMessages.ApiTokenSet(_logger);
    }

    public void ClearToken()
    {
        Token = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
        InfrastructureHttpLogMessages.ApiTokenCleared(_logger);
    }

    public Task<ErrorOr<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
        => SendAsync<TResponse>(
            "GET",
            endpoint,
            async cancellationTokenValue => await _httpClient.GetAsync(endpoint, cancellationTokenValue),
            cancellationToken);

    public Task<ErrorOr<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest? data, CancellationToken cancellationToken = default)
        => SendAsync<TResponse>(
            "POST",
            endpoint,
            async cancellationTokenValue => await _httpClient.PostAsJsonAsync(endpoint, data, cancellationTokenValue),
            cancellationToken);

    public Task<ErrorOr<Success>> PostAsync<TRequest>(string endpoint, TRequest? data, CancellationToken cancellationToken = default)
        => SendSuccessAsync(
            "POST",
            endpoint,
            async cancellationTokenValue => await _httpClient.PostAsJsonAsync(endpoint, data, cancellationTokenValue),
            cancellationToken);

    public Task<ErrorOr<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest? data, CancellationToken cancellationToken = default)
        => SendAsync<TResponse>(
            "PUT",
            endpoint,
            async cancellationTokenValue => await _httpClient.PutAsJsonAsync(endpoint, data, cancellationTokenValue),
            cancellationToken);

    public Task<ErrorOr<Success>> PutAsync<TRequest>(string endpoint, TRequest? data, CancellationToken cancellationToken = default)
        => SendSuccessAsync(
            "PUT",
            endpoint,
            async cancellationTokenValue => await _httpClient.PutAsJsonAsync(endpoint, data, cancellationTokenValue),
            cancellationToken);

    public Task<ErrorOr<Success>> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
        => SendSuccessAsync(
            "DELETE",
            endpoint,
            async cancellationTokenValue => await _httpClient.DeleteAsync(endpoint, cancellationTokenValue),
            cancellationToken);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _httpClient.Dispose();
        _disposed = true;
    }

    private async Task<ErrorOr<TResponse>> SendAsync<TResponse>(
        string operation,
        string endpoint,
        Func<CancellationToken, Task<HttpResponseMessage>> send,
        CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await send(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return await MapErrorAsync(operation, endpoint, response, cancellationToken);
            }

            if (typeof(TResponse) == typeof(string))
            {
                string raw = await response.Content.ReadAsStringAsync(cancellationToken);
                string trimmed = raw.Trim('"');
                return (TResponse)(object)trimmed;
            }

            TResponse? result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
            if (result is null)
            {
                InfrastructureHttpLogMessages.HttpEmptyResponse(_logger, operation);
                return Error.Failure("Api.EmptyResponse", "The API returned an empty response.");
            }

            return result;
        }
        catch (HttpRequestException exception)
        {
            InfrastructureHttpLogMessages.HttpRequestTransportFailed(_logger, exception, operation);
            return Error.Failure(description: exception.Message);
        }
        catch (OperationCanceledException)
        {
            InfrastructureHttpLogMessages.HttpRequestCancelled(_logger, operation);
            return Error.Unexpected("Api.RequestCancelled", "The API request was cancelled.");
        }
    }

    private async Task<ErrorOr<Success>> SendSuccessAsync(
        string operation,
        string endpoint,
        Func<CancellationToken, Task<HttpResponseMessage>> send,
        CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await send(cancellationToken);
            return response.IsSuccessStatusCode ? Result.Success : await MapErrorAsync(operation, endpoint, response, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            InfrastructureHttpLogMessages.HttpRequestTransportFailed(_logger, exception, operation);
            return Error.Failure(description: exception.Message);
        }
        catch (OperationCanceledException)
        {
            InfrastructureHttpLogMessages.HttpRequestCancelled(_logger, operation);
            return Error.Unexpected("Api.RequestCancelled", "The API request was cancelled.");
        }
    }

    private async Task<Error> MapErrorAsync(string operation, string endpoint, HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string responseBody;
        try
        {
            responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch
        {
            responseBody = string.Empty;
        }

        string description = TryExtractProblemDetailsMessage(responseBody)
            ?? (string.IsNullOrWhiteSpace(responseBody)
            ? response.ReasonPhrase ?? "Request failed."
            : responseBody);

        InfrastructureHttpLogMessages.HttpRequestFailed(
            _logger,
            operation,
            (int)response.StatusCode,
            GetLogDescription(endpoint, description));

        return response.StatusCode switch
        {
            HttpStatusCode.BadRequest => Error.Validation(description: description),
            HttpStatusCode.Unauthorized => Error.Unauthorized(description: description),
            HttpStatusCode.Forbidden => Error.Forbidden(description: description),
            HttpStatusCode.NotFound => Error.NotFound(description: description),
            HttpStatusCode.Conflict => Error.Conflict(description: description),
            HttpStatusCode.UnprocessableEntity => Error.Validation(description: description),
            >= HttpStatusCode.InternalServerError => Error.Unexpected(description: description),
            _ => Error.Failure(description: description),
        };
    }

    private static string? TryExtractProblemDetailsMessage(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(responseBody);
            JsonElement root = jsonDocument.RootElement;

            if (root.TryGetProperty("detail", out JsonElement detailElement)
                && detailElement.ValueKind == JsonValueKind.String)
            {
                string? detail = detailElement.GetString();
                if (!string.IsNullOrWhiteSpace(detail))
                {
                    return detail;
                }
            }

            if (root.TryGetProperty("title", out JsonElement titleElement)
                && titleElement.ValueKind == JsonValueKind.String)
            {
                string? title = titleElement.GetString();
                if (!string.IsNullOrWhiteSpace(title))
                {
                    return title;
                }
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    private static string GetLogDescription(string endpoint, string description)
        => IsSensitiveEndpoint(endpoint) ? "Response redacted for sensitive endpoint." : description;

    private static bool IsSensitiveEndpoint(string endpoint)
        => endpoint.Equals(ApiEndpoints.Profile.ChangePasswordFull, StringComparison.OrdinalIgnoreCase)
           || endpoint.Equals(ApiEndpoints.Profile.VerifyPasswordFull, StringComparison.OrdinalIgnoreCase);

    private static HttpClient CreateStandaloneClient(IConfiguration configuration)
    {
        string? baseUrl = configuration["ApiBaseUrl"];
        return string.IsNullOrWhiteSpace(baseUrl)
            ? new HttpClient()
            : new HttpClient { BaseAddress = new Uri(baseUrl) };
    }
}
