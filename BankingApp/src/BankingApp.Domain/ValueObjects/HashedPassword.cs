namespace BankingApp.Domain.ValueObjects;

using Common.Primitives;

public sealed record HashedPassword : ValueObject
{
    private HashedPassword()
    {
    }

    public string Value { get; private init; } = string.Empty;

    public static HashedPassword Wrap(string hash) => new() { Value = hash };

    public override string ToString() => Value;
}
