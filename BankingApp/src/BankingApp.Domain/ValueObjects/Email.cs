namespace BankingApp.Domain.ValueObjects;

using Common.Errors;
using Common.Primitives;
using EmailValidation;
using ErrorOr;

public sealed record Email : ValueObject
{
    private Email()
    {
    }

    public string Value { get; private init; } = string.Empty;

    public static ErrorOr<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return UserErrors.InvalidEmail;
        }

        string normalized = value.Trim();

        if (normalized.Length > 254 || !EmailValidator.Validate(normalized))
        {
            return UserErrors.InvalidEmail;
        }

        return new Email { Value = normalized };
    }

    public override string ToString() => Value;
}
