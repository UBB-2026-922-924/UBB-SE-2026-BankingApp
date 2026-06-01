namespace BankingApp.Web.Models.Investments;

using BankingApp.Contracts.Features.Investments.Dtos;

public sealed class InvestmentsPageViewModel
{
    public PortfolioDto Portfolio { get; init; } = new();
}
