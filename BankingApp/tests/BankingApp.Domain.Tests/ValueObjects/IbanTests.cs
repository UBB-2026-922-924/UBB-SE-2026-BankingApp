namespace BankingApp.Domain.Tests.ValueObjects;

public sealed class IbanTests
{
    [Fact(Skip = "Not implemented yet.")]
    public void Create_WhenValueIsTooShort_ShouldReturnInvalidIbanError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Create_WhenValueIsTooLong_ShouldReturnInvalidIbanError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Create_WhenFirstTwoCharsAreNotLetters_ShouldReturnInvalidIbanError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Create_WhenCheckDigitsAreNotNumeric_ShouldReturnInvalidIbanError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Create_WhenValueIsValid_ShouldReturnIban()
    {
        throw new NotImplementedException();
    }
}
