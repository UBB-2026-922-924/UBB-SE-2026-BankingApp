namespace BankingApp.Application.Features.Authentication.Services;

using System.Transactions;
using Common.Security;
using Contracts.Features.Authentication.Dtos;
using Domain.Aggregates.IdentityAggregate;
using Domain.Aggregates.IdentityAggregate.Entities;
using Domain.Aggregates.UserAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Models;
using Shared.Clock;
using Shared.Persistence;
using ApplicationLogMessages = Common.Logging.ApplicationLogMessages;

public sealed class AuthService(
    IUserRepository userRepository,
    IIdentityRepository identityRepository,
    IHashService hashService,
    IJsonWebTokenService jwtService,
    IUnitOfWork unitOfWork,
    ISystemClock clock,
    ILogger<AuthService> logger)
    : IAuthService
{
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 15;
    private const int SessionExpiryHours = 24;

    public async Task<ErrorOr<LoginSuccess>> LoginAsync(string email, string password, SessionMetadata? metadata, CancellationToken cancellationToken = default)
    {
        ErrorOr<Email> emailResult = Email.Create(email);
        if (emailResult.IsError)
        {
            return AuthErrors.InvalidCredentials;
        }

        ErrorOr<(User User, IdentityAccount Identity)> loginContextResult =
            await LoadLoginContextAsync(emailResult.Value, cancellationToken);
        if (loginContextResult.IsError)
        {
            return loginContextResult.FirstError;
        }

        User user = loginContextResult.Value.User;
        IdentityAccount identity = loginContextResult.Value.Identity;

        if (identity.IsLocked)
        {
            if (identity.IsCurrentlyLocked())
            {
                ApplicationLogMessages.LoginBlockedLockedAccount(logger, user.Id, identity.LockoutEnd);
                return AuthErrors.AccountLocked;
            }

            identity.ResetFailedAttempts();
            await identityRepository.UpdateAsync(identity, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        if (identity.PasswordHash is null)
        {
            ApplicationLogMessages.LoginOAuthOnlyPasswordRejected(logger, user.Id);
            return AuthErrors.InvalidCredentials;
        }

        ErrorOr<bool> verifyResult = hashService.Verify(password, identity.PasswordHash.Value);
        if (verifyResult.IsError)
        {
            ApplicationLogMessages.PasswordHashVerificationFailed(logger, user.Id, verifyResult.FirstError.Description);
            return verifyResult.FirstError;
        }

        if (!verifyResult.Value)
        {
            return await HandleFailedPasswordAsync(identity, user.Id, cancellationToken);
        }

        return await CompleteLoginAsync(user, identity, metadata, cancellationToken);
    }

    public async Task<ErrorOr<Success>> LogoutAsync(string token, CancellationToken cancellationToken = default)
    {
        IdentityAccount? identity = await identityRepository.GetBySessionTokenAsync(token, cancellationToken);
        if (identity is null)
        {
            ApplicationLogMessages.LogoutSessionNotFound(logger);
            return AuthErrors.SessionNotFound;
        }

        Session? session = identity.Sessions.FirstOrDefault(s => s.Token == token && !s.IsRevoked);
        if (session is null)
        {
            ApplicationLogMessages.LogoutSessionNotFound(logger);
            return AuthErrors.SessionNotFound;
        }

        session.Revoke();
        await identityRepository.UpdateAsync(identity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ApplicationLogMessages.UserLoggedOut(logger, identity.UserId);
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> RegisterAsync(string email, string password, string fullName, CancellationToken cancellationToken = default)
    {
        ErrorOr<Email> emailResult = Email.Create(email);
        if (emailResult.IsError)
        {
            return emailResult.FirstError;
        }

        ErrorOr<Success> emailAvailabilityResult = await EnsureEmailAvailableAsync(emailResult.Value, cancellationToken);
        if (emailAvailabilityResult.IsError)
        {
            return emailAvailabilityResult.FirstError;
        }

        ErrorOr<string> hashResult = hashService.GetHash(password);
        if (hashResult.IsError)
        {
            ApplicationLogMessages.RegistrationHashGenerationFailed(logger);
            return hashResult.FirstError;
        }

        return await RegisterUserAsync(emailResult.Value, fullName, hashResult.Value, cancellationToken);
    }

    private async Task<ErrorOr<(User User, IdentityAccount Identity)>> LoadLoginContextAsync(Email email, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            ApplicationLogMessages.LoginUserNotFoundForEmail(logger);
            return AuthErrors.InvalidCredentials;
        }

        IdentityAccount? identity = await identityRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (identity is null)
        {
            return AuthErrors.InvalidCredentials;
        }

        return (user, identity);
    }

    private async Task<Error> HandleFailedPasswordAsync(IdentityAccount identity, int userId, CancellationToken cancellationToken)
    {
        identity.IncrementFailedAttempts();
        int attempts = identity.FailedLoginAttempts;
        ApplicationLogMessages.FailedLoginAttempt(logger, userId, attempts, MaxFailedAttempts);

        if (attempts < MaxFailedAttempts)
        {
            await identityRepository.UpdateAsync(identity, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return AuthErrors.InvalidCredentials;
        }

        identity.LockAccount(clock.UtcNow.AddMinutes(LockoutMinutes));
        await identityRepository.UpdateAsync(identity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ApplicationLogMessages.AccountLockedTooManyAttempts(logger, userId, LockoutMinutes, MaxFailedAttempts);
        return AuthErrors.AccountLockedTooManyAttempts;
    }

    private async Task<ErrorOr<LoginSuccess>> CompleteLoginAsync(User user, IdentityAccount identity, SessionMetadata? metadata, CancellationToken cancellationToken)
    {
        identity.ResetFailedAttempts();

        ErrorOr<string> tokenResult = jwtService.GenerateToken(user.Id);
        if (tokenResult.IsError)
        {
            ApplicationLogMessages.TokenGenerationFailed(logger, user.Id, tokenResult.FirstError.Description);
            return tokenResult.FirstError;
        }

        string token = tokenResult.Value;
        DateTime now = clock.UtcNow;
        Session session = identity.OpenSession(token, now.AddHours(SessionExpiryHours), now, metadata?.DeviceInfo, metadata?.Browser, metadata?.IpAddress);

        await identityRepository.UpdateAsync(identity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ApplicationLogMessages.UserLoggedIn(logger, user.Id);
        return new LoginSuccess(user.Id, token, session.Id);
    }

    private async Task<ErrorOr<Success>> EnsureEmailAvailableAsync(Email email, CancellationToken cancellationToken)
    {
        User? existing = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (existing is not null)
        {
            ApplicationLogMessages.RegistrationRejectedEmailAlreadyRegistered(logger);
            return AuthErrors.EmailAlreadyRegistered;
        }

        return Result.Success;
    }

    private async Task<ErrorOr<Success>> RegisterUserAsync(Email email, string fullName, string passwordHash, CancellationToken cancellationToken)
    {
        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        DateTime now = clock.UtcNow;
        var user = User.Register(email, fullName.Trim(), now);
        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var identity = IdentityAccount.Create(user.Id, HashedPassword.Wrap(passwordHash));
        await identityRepository.AddAsync(identity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        transactionScope.Complete();
        ApplicationLogMessages.UserRegisteredSuccessfully(logger);
        return Result.Success;
    }
}
