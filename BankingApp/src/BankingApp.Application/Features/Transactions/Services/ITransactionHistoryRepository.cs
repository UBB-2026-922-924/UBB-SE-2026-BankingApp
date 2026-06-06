namespace BankingApp.Application.Features.Transactions.Services;

using BankingApp.Contracts.Features.Transactions.Dtos;
using BankingApp.Domain.Aggregates.AccountAggregate;
using BankingApp.Domain.Aggregates.AccountAggregate.Entities;

/// <summary>
///     Read-side repository for the transaction history feature.
///     Returns flat DTOs optimised for the history view rather than domain aggregates.
/// </summary>
public interface ITransactionHistoryRepository
{
    public List<TransactionHistoryItemDto> GetTransactionsByUserId(int userId);
    public TransactionHistoryItemDto? GetTransactionById(int userId, int transactionId);
    public List<Account> GetAccountsByUserId(int userId);
    public List<Card> GetCardsByUserId(int userId);
}