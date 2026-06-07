namespace BankingApp.Domain.Aggregates.IdentityAggregate;

using Entities;
using Common.Primitives;
using ValueObjects;

public sealed class IdentityAccount : AggregateRoot<int>
{
    private readonly List<Session> _sessions = [];

    private IdentityAccount()
    {
    }

    public int UserId { get; private set; }

    public HashedPassword? PasswordHash { get; private set; }

    public bool IsLocked { get; private set; }

    public DateTime? LockoutEnd { get; private set; }

    public int FailedLoginAttempts { get; private set; }

    public bool Is2FaEnabled { get; private set; }

    public string? Preferred2FaMethod { get; private set; }

    public IReadOnlyCollection<Session> Sessions => _sessions.AsReadOnly();

    public static IdentityAccount Create(int userId, HashedPassword? passwordHash, bool is2FaEnabled = false, string? preferred2FaMethod = null)
    {
        return new IdentityAccount
        {
            UserId = userId,
            PasswordHash = passwordHash,
            Is2FaEnabled = is2FaEnabled,
            Preferred2FaMethod = preferred2FaMethod
        };
    }

    public bool IsCurrentlyLocked(DateTime now) => IsLocked && LockoutEnd.HasValue && LockoutEnd.Value > now;

    public void IncrementFailedAttempts() => FailedLoginAttempts++;

    public void LockAccount(DateTime lockoutEnd)
    {
        IsLocked = true;
        LockoutEnd = lockoutEnd;
    }

    public void ResetFailedAttempts()
    {
        FailedLoginAttempts = 0;
        IsLocked = false;
        LockoutEnd = null;
    }

    public void UpdatePassword(HashedPassword hash) => PasswordHash = hash;

    public bool TryRevokeSession(int sessionId)
    {
        Session? session = _sessions.FirstOrDefault(s => s.Id == sessionId);
        if (session is null)
        {
            return false;
        }

        session.Revoke();
        return true;
    }

    public void InvalidateAllSessions()
    {
        foreach (Session session in _sessions.Where(s => !s.IsRevoked))
        {
            session.Revoke();
        }
    }

    public Session OpenSession(string token, DateTime expiresAt, DateTime createdAt, string? deviceInfo = null, string? browser = null, string? ipAddress = null)
    {
        var session = Session.Create(Id, token, expiresAt, createdAt, deviceInfo, browser, ipAddress);
        _sessions.Add(session);
        return session;
    }

    public void Update2FaPreferences(bool enabled, string? method)
    {
        Is2FaEnabled = enabled;
        Preferred2FaMethod = method;
    }

}
