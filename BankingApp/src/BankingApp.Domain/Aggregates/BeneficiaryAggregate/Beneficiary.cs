namespace BankingApp.Domain.Aggregates.BeneficiaryAggregate;

using Common.Primitives;
using ValueObjects;

public sealed class Beneficiary : AggregateRoot<int>
{
    private Beneficiary()
    {
    }

    public int UserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public Iban Iban { get; private set; } = default!;

    public string? BankName { get; private set; }

    public DateTime? LastTransferDate { get; private set; }

    public decimal TotalAmountSent { get; private set; }

    public int TransferCount { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static Beneficiary Create(int userId, string name, Iban iban, string? bankName, DateTime createdAt)
    {
        return new Beneficiary
        {
            UserId = userId,
            Name = name,
            Iban = iban,
            BankName = bankName,
            CreatedAt = createdAt
        };
    }

    public void Update(string name, Iban iban, string? bankName)
    {
        Name = name;
        Iban = iban;
        BankName = bankName;
    }

    public void RegisterTransfer(decimal amount, DateTime transferredAt)
    {
        LastTransferDate = transferredAt;
        TotalAmountSent += amount;
        TransferCount++;
    }
}
