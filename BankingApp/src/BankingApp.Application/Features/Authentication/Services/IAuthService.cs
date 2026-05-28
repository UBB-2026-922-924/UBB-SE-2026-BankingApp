namespace BankingApp.Application.Features.Authentication.Services;

using Contracts.Features.Authentication.Dtos;
using ErrorOr;
using Models;

public interface IAuthService
{
    public Task<ErrorOr<LoginSuccess>> LoginAsync(string email, string password, SessionMetadata? metadata, CancellationToken cancellationToken = default);
    public Task<ErrorOr<Success>> LogoutAsync(string token, CancellationToken cancellationToken = default);
    public Task<ErrorOr<Success>> RegisterAsync(string email, string password, string fullName, CancellationToken cancellationToken = default);
}
