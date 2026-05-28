namespace BankingApp.Domain.Tests.Aggregates.AccountAggregate;

public sealed class AccountTests
{
    [Fact(Skip = "Not implemented yet.")]
    public void Debit_WhenAmountIsNegative_ShouldReturnNegativeAmountError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Debit_WhenCurrencyMismatches_ShouldReturnCurrencyMismatchError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Debit_WhenInsufficientFunds_ShouldReturnInsufficientFundsError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Debit_WhenValidAmount_ShouldDecreaseBalance()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Debit_WhenValidAmount_ShouldRaiseBalanceUpdatedEvent()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Credit_WhenAmountIsNegative_ShouldReturnNegativeAmountError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Credit_WhenCurrencyMismatches_ShouldReturnCurrencyMismatchError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Credit_WhenValidAmount_ShouldIncreaseBalance()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Credit_WhenValidAmount_ShouldRaiseBalanceUpdatedEvent()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Rename_WhenCalled_ShouldUpdateAccountName()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void ChangeBalance_WhenCalled_ShouldUpdateBalance()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void ChangeBalance_WhenCalled_ShouldRaiseBalanceUpdatedEvent()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void IssueCard_WhenCalled_ShouldAddCardToCardsCollection()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void RecordTransaction_WhenCalled_ShouldAddTransactionToTransactionsCollection()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void RecordTransaction_WhenCalled_ShouldRaiseTransactionRecordedEvent()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void HasSufficientFunds_WhenBalanceCoversAmount_ShouldReturnTrue()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void HasSufficientFunds_WhenBalanceDoesNotCoverAmount_ShouldReturnFalse()
    {
        throw new NotImplementedException();
    }
}
