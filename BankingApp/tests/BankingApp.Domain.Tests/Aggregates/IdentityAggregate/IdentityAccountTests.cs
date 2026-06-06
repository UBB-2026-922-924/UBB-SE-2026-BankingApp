namespace BankingApp.Domain.Tests.Aggregates.IdentityAggregate;

using BankingApp.Domain.Aggregates.IdentityAggregate;

public sealed class IdentityAccountTests
{
    private static readonly DateTime _now = new(2026, 6, 5, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void IsCurrentlyLocked_WhenLockedWithFutureLockoutEnd_ShouldReturnTrue()
    {
        // Arrange
        var identity = IdentityAccount.Create(userId: 1, passwordHash: null);
        identity.LockAccount(_now.AddMinutes(15));

        // Act
        bool isLocked = identity.IsCurrentlyLocked(_now);

        // Assert
        isLocked.Should().BeTrue();
    }

    [Fact]
    public void IsCurrentlyLocked_WhenLockedWithPastLockoutEnd_ShouldReturnFalse()
    {
        // Arrange
        var identity = IdentityAccount.Create(userId: 1, passwordHash: null);
        identity.LockAccount(_now.AddMinutes(-1));

        // Act
        bool isLocked = identity.IsCurrentlyLocked(_now);

        // Assert
        isLocked.Should().BeFalse();
    }

    [Fact]
    public void IsCurrentlyLocked_WhenNotLocked_ShouldReturnFalse()
    {
        // Arrange
        var identity = IdentityAccount.Create(userId: 1, passwordHash: null);

        // Act
        bool isLocked = identity.IsCurrentlyLocked(_now);

        // Assert
        isLocked.Should().BeFalse();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void IncrementFailedAttempts_WhenCalled_ShouldIncrementCounter()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void LockAccount_WhenCalled_ShouldSetIsLockedToTrue()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void LockAccount_WhenCalled_ShouldSetLockoutEnd()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void ResetFailedAttempts_WhenCalled_ShouldSetCounterToZero()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void ResetFailedAttempts_WhenCalled_ShouldUnlockAccount()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void UpdatePassword_WhenCalled_ShouldReplacePasswordHash()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void TryRevokeSession_WhenSessionExists_ShouldRevokeItAndReturnTrue()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void TryRevokeSession_WhenSessionDoesNotExist_ShouldReturnFalse()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void InvalidateAllSessions_WhenCalled_ShouldRevokeAllNonRevokedSessions()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void InvalidateAllSessions_WhenCalled_ShouldNotAffectAlreadyRevokedSessions()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void OpenSession_WhenCalled_ShouldAddSessionToCollection()
    {
        throw new NotImplementedException();
    }

}
