namespace BankingApp.Domain.Tests.Aggregates.ForexAggregate;

public sealed class ForexTransactionTests
{
    [Fact(Skip = "Not implemented yet.")]
    public void Create_WhenSourceAmountIsZero_ShouldReturnInvalidAmountError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Create_WhenTargetAmountIsNegative_ShouldReturnInvalidAmountError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Create_WhenSourceAndTargetCurrencyAreTheSame_ShouldReturnSameCurrencyError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Create_WhenExchangeRateIsZero_ShouldReturnInvalidRateError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Create_WhenCommissionIsNegative_ShouldReturnInvalidCommissionError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Create_WhenCommissionCurrencyDiffersFromSource_ShouldReturnCurrencyMismatchError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Create_WhenValidParams_ShouldCreateWithPendingStatus()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void LockRate_WhenCalled_ShouldSetRateLockedAt()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void MarkExecuted_WhenCalled_ShouldSetStatusToCompleted()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void MarkExecuted_WhenCalled_ShouldSetLedgerTransactionIds()
    {
        throw new NotImplementedException();
    }
}
