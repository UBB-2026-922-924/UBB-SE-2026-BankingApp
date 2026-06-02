namespace BankingApp.Application.Features.Investments.Services
{
    using System.Threading.Tasks;
    using BankingApp.Domain.Aggregates.InvestmentAggregate;

    public interface IInvestmentsService
    {
        Task<Portfolio?> GetPortfolioAsync(int userId);
        Task<Portfolio?> GetPortfolioForCurrentUserAsync();
        Task<bool> ExecuteTradeAsync(int userId, string ticker, string action, decimal quantity, decimal price);
    }
}