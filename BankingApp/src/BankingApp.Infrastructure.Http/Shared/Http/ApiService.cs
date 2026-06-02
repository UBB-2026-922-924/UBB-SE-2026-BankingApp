namespace BankingApp.Infrastructure.Http.Shared.Http;

using System.Net;
using Application.Shared.Http;
using ErrorOr;

public sealed class ApiService(IApiClient apiClient)
{
    public async Task<TResponse> GetAsync<TResponse>(string endpoint)
    {
        ErrorOr<TResponse> result = await apiClient.GetAsync<TResponse>(NormalizeEndpoint(endpoint));
        return Unwrap(result);
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        ErrorOr<TResponse> result = await apiClient.PostAsync<TRequest, TResponse>(NormalizeEndpoint(endpoint), data);
        return Unwrap(result);
    }

    public async Task PostAsync<TRequest>(string endpoint, TRequest data)
    {
        ErrorOr<Success> result = await apiClient.PostAsync(NormalizeEndpoint(endpoint), data);
        Unwrap(result);
    }

    public async Task<TResponse> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        ErrorOr<TResponse> result = await apiClient.PutAsync<TRequest, TResponse>(NormalizeEndpoint(endpoint), data);
        return Unwrap(result);
    }

    public async Task PutAsync<TRequest>(string endpoint, TRequest data)
    {
        ErrorOr<Success> result = await apiClient.PutAsync(NormalizeEndpoint(endpoint), data);
        Unwrap(result);
    }

    private static string NormalizeEndpoint(string endpoint)
    {
        return endpoint.TrimStart('/');
    }

    private static TResponse Unwrap<TResponse>(ErrorOr<TResponse> result)
    {
        if (!result.IsError)
        {
            return result.Value;
        }

        Error error = result.FirstError;
        throw new HttpRequestException(error.Description, inner: null, GetStatusCode(error.Type));
    }

    private static HttpStatusCode GetStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.Unauthorized => HttpStatusCode.Unauthorized,
        ErrorType.Forbidden => HttpStatusCode.Forbidden,
        ErrorType.Validation => HttpStatusCode.BadRequest,
        ErrorType.Conflict => HttpStatusCode.Conflict,
        ErrorType.NotFound => HttpStatusCode.NotFound,
        _ => HttpStatusCode.InternalServerError,
    };
}
