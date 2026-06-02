namespace BankingApp.Web.ViewModels.Shared;

using Contracts.Features.AccountOverview.Dtos;

public sealed class AccountCardViewModel
{
    public string AccountName { get; init; } = string.Empty;
    public string MaskedCardNumber { get; init; } = string.Empty;
    public decimal Balance { get; init; }
    public string CardType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;

    public bool IsActive => Status is "Active";

    public static AccountCardViewModel FromCard(CardDto card) =>
        new()
        {
            AccountName      = card.AccountName ?? string.Empty,
            MaskedCardNumber = card.CardNumber,
            Balance          = card.AccountBalance ?? 0m,
            CardType         = card.CardType.ToString(),
            Status           = card.Status.ToString()
        };
}
