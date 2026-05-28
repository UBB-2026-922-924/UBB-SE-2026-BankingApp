namespace BankingApp.Domain.Tests.Aggregates.IdentityAggregate;

public sealed class IdentityAccountTests
{
    [Fact(Skip = "Not implemented yet.")]
    public void IsCurrentlyLocked_WhenLockedWithFutureLockoutEnd_ShouldReturnTrue()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void IsCurrentlyLocked_WhenLockedWithPastLockoutEnd_ShouldReturnFalse()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void IsCurrentlyLocked_WhenNotLocked_ShouldReturnFalse()
    {
        throw new NotImplementedException();
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
