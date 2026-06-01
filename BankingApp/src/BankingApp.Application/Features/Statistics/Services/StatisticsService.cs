namespace BankingApp.Application.Features.Statistics.Services;

using Contracts.Features.Statistics.Dtos;
using Domain.Aggregates.AccountAggregate.Entities;
using Domain.Enums;
using Domain.Repositories;
using ErrorOr;

public sealed class StatisticsService(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository)
    : IStatisticsService
{
    private const int BalanceTrendDays = 70;
    private const int TopRecipientsCount = 5;

    public async Task<ErrorOr<SpendingByCategoryResponse>> GetSpendingByCategoryAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var spendingTransactions = (await GetAnalyticsTransactionsAsync(userId, cancellationToken))
            .Where(transaction => transaction.Direction == TransactionDirection.Out)
            .ToList();

        decimal totalSpending = spendingTransactions.Sum(transaction => transaction.Amount.Amount);
        var categories = spendingTransactions
            .GroupBy(transaction => transaction.CategoryId?.ToString() ?? "Uncategorized")
            .Select(group => new CategorySpendingPointDto
            {
                CategoryName = group.Key,
                Amount = group.Sum(transaction => transaction.Amount.Amount),
            })
            .OrderByDescending(category => category.Amount)
            .ToList();

        foreach (CategorySpendingPointDto category in categories)
        {
            category.ShareOfTotal = totalSpending == 0 ? 0 : Math.Round(category.Amount / totalSpending, 4);
        }

        return new SpendingByCategoryResponse
        {
            Success = true,
            Message = "Spending by category loaded successfully.",
            TotalSpending = totalSpending,
            Categories = categories,
        };
    }

    public async Task<ErrorOr<IncomeVsExpensesResponse>> GetIncomeVsExpensesAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Transaction> transactions = await GetAnalyticsTransactionsAsync(userId, cancellationToken);
        decimal income = transactions
            .Where(transaction => transaction.Direction == TransactionDirection.In)
            .Sum(transaction => transaction.Amount.Amount);
        decimal expenses = transactions
            .Where(transaction => transaction.Direction == TransactionDirection.Out)
            .Sum(transaction => transaction.Amount.Amount);

        return new IncomeVsExpensesResponse
        {
            Success = true,
            Message = "Income and expenses loaded successfully.",
            Income = income,
            Expenses = expenses,
            Net = income - expenses,
        };
    }

    public async Task<ErrorOr<BalanceTrendsResponse>> GetBalanceTrendsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        DateTime cutoffDate = DateTime.UtcNow.Date.AddDays(-BalanceTrendDays);
        var points = (await GetAnalyticsTransactionsAsync(userId, cancellationToken))
            .Where(transaction => transaction.CreatedAt.Date >= cutoffDate)
            .GroupBy(transaction => transaction.CreatedAt.Date)
            .Select(group => group.OrderByDescending(transaction => transaction.CreatedAt).ThenByDescending(transaction => transaction.Id).First())
            .OrderBy(transaction => transaction.CreatedAt.Date)
            .Select(transaction => new BalanceTrendPointDto
            {
                Date = transaction.CreatedAt.Date,
                Balance = transaction.BalanceAfter.Amount,
            })
            .ToList();

        return new BalanceTrendsResponse
        {
            Success = true,
            Message = "Balance trends loaded successfully.",
            Points = points,
        };
    }

    public async Task<ErrorOr<TopRecipientsResponse>> GetTopRecipientsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var recipients = (await GetAnalyticsTransactionsAsync(userId, cancellationToken))
            .Where(transaction => transaction.Direction == TransactionDirection.Out)
            .Where(transaction => !string.IsNullOrWhiteSpace(transaction.CounterpartyName ?? transaction.MerchantName))
            .GroupBy(transaction => transaction.CounterpartyName ?? transaction.MerchantName)
            .Select(group => new TopCounterpartyDto
            {
                Name = group.Key!,
                TotalAmount = group.Sum(transaction => transaction.Amount.Amount),
                TransactionCount = group.Count(),
            })
            .OrderByDescending(recipient => recipient.TotalAmount)
            .ThenBy(recipient => recipient.Name, StringComparer.OrdinalIgnoreCase)
            .Take(TopRecipientsCount)
            .ToList();

        return new TopRecipientsResponse
        {
            Success = true,
            Message = "Top recipients loaded successfully.",
            Recipients = recipients,
        };
    }

    private async Task<IReadOnlyCollection<Transaction>> GetAnalyticsTransactionsAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Domain.Aggregates.AccountAggregate.Account> accounts =
            await accountRepository.ListByUserIdAsync(userId, cancellationToken);

        var transactions = new List<Transaction>();
        foreach (Domain.Aggregates.AccountAggregate.Account account in accounts)
        {
            IReadOnlyCollection<Transaction> accountTransactions =
                await transactionRepository.ListByAccountIdAsync(account.Id, cancellationToken);
            transactions.AddRange(accountTransactions.Where(transaction => transaction.Status == TransactionStatus.Completed));
        }

        return transactions;
    }
}
