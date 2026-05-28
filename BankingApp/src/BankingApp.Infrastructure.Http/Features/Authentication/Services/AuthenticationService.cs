namespace BankingApp.Infrastructure.Http.Features.Authentication.Services;

using Application.Features.Authentication.Services;
using Application.Shared.Http;
using Contracts.Features.Authentication.Dtos;
using Contracts.Features.UserRegistration.Dtos;
using Contracts.Http;
using ErrorOr;

public sealed class AuthenticationService(IApiClient apiClient) : IAuthenticationService
{
    public Task<ErrorOr<LoginSuccessResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        => apiClient.PostAsync<LoginRequest, LoginSuccessResponse>(ApiEndpoints.Auth.LoginFull, request, cancellationToken);

    public Task<ErrorOr<Success>> LogoutAsync(CancellationToken cancellationToken = default)
        => apiClient.PostAsync(ApiEndpoints.Auth.LogoutFull, new { }, cancellationToken);

    public Task<ErrorOr<Success>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        => apiClient.PostAsync(ApiEndpoints.Auth.RegisterFull, request, cancellationToken);
}
