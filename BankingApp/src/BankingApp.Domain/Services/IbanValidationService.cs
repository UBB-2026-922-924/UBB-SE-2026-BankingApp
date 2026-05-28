namespace BankingApp.Domain.Services;

public static class IbanValidationService
{
    public static bool IsValid(string iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
        {
            return false;
        }

        return iban.Length is >= 15 and <= 34
               && char.IsLetter(iban[0])
               && char.IsLetter(iban[1])
               && char.IsDigit(iban[2])
               && char.IsDigit(iban[3]);
    }
}
