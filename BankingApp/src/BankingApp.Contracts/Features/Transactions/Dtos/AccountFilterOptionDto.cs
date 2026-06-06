namespace BankingApp.Contracts.Features.Transactions.Dtos;

/// <summary>Represents an account available as a filter option in the transaction history view.</summary>
public sealed class AccountFilterOptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Iban { get; set; } = string.Empty;
}