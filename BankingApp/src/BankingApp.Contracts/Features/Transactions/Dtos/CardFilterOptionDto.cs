namespace BankingApp.Contracts.Features.Transactions.Dtos;

/// <summary>Represents a card available as a filter option in the transaction history view.</summary>
public sealed class CardFilterOptionDto
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
}