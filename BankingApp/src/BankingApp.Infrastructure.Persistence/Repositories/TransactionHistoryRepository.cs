namespace BankingApp.Infrastructure.Persistence.Repositories;

using Application.Features.Transactions.Services;
using BankingApp.Contracts.Features.Transactions.Dtos;
using Data;
using Domain.Aggregates.AccountAggregate;
using Domain.Aggregates.AccountAggregate.Entities;
using Microsoft.EntityFrameworkCore;

public sealed class TransactionHistoryRepository(AppDbContext dbContext) : ITransactionHistoryRepository
{
    public List<TransactionHistoryItemDto> GetTransactionsByUserId(int userId)
    {
        return dbContext.Accounts
            .Where(account => account.UserId == userId)
            .Include(account => account.Cards)
            .Include(account => account.Transactions)
            .SelectMany(account => account.Transactions.Select(transaction => new TransactionHistoryItemDto
            {
                Id = transaction.Id,
                AccountId = account.Id,
                CardId = transaction.CardId,
                AccountName = account.AccountName ?? $"Account {account.Id}",
                AccountIban = account.Iban.Value,
                Timestamp = transaction.CreatedAt,
                TransactionType = transaction.Type,
                ReferenceNumber = transaction.TransactionRef,
                Description = transaction.Description,
                CounterpartyOrMerchant = transaction.CounterpartyName ?? transaction.MerchantName ?? string.Empty,
                MerchantName = transaction.MerchantName,
                CounterpartyName = transaction.CounterpartyName,
                Amount = transaction.Amount.Amount,
                Currency = transaction.Amount.Currency.Code,
                Direction = transaction.Direction.ToString(),
                RunningBalanceAfterTransaction = transaction.BalanceAfter.Amount,
                Status = transaction.Status.ToString(),
                Fee = transaction.Fee.HasValue ? transaction.Fee.Value.Amount : decimal.Zero,
                ExchangeRate = transaction.ExchangeRate
            }))
            .ToList();
    }

    public TransactionHistoryItemDto? GetTransactionById(int userId, int transactionId)
    {
        return dbContext.Accounts
            .Where(account => account.UserId == userId)
            .Include(account => account.Transactions)
            .SelectMany(account => account.Transactions.Select(transaction => new TransactionHistoryItemDto
            {
                Id = transaction.Id,
                AccountId = account.Id,
                CardId = transaction.CardId,
                AccountName = account.AccountName ?? $"Account {account.Id}",
                AccountIban = account.Iban.Value,
                Timestamp = transaction.CreatedAt,
                TransactionType = transaction.Type,
                ReferenceNumber = transaction.TransactionRef,
                Description = transaction.Description,
                CounterpartyOrMerchant = transaction.CounterpartyName ?? transaction.MerchantName ?? string.Empty,
                MerchantName = transaction.MerchantName,
                CounterpartyName = transaction.CounterpartyName,
                Amount = transaction.Amount.Amount,
                Currency = transaction.Amount.Currency.Code,
                Direction = transaction.Direction.ToString(),
                RunningBalanceAfterTransaction = transaction.BalanceAfter.Amount,
                Status = transaction.Status.ToString(),
                Fee = transaction.Fee.HasValue ? transaction.Fee.Value.Amount : decimal.Zero,
                ExchangeRate = transaction.ExchangeRate
            }))
            .FirstOrDefault(dto => dto.Id == transactionId);
    }

    public List<Account> GetAccountsByUserId(int userId)
    {
        return dbContext.Accounts
            .Where(account => account.UserId == userId)
            .Include(account => account.Cards)
            .OrderBy(account => account.Id)
            .ToList();
    }

    public List<Card> GetCardsByUserId(int userId)
    {
        return dbContext.Accounts
            .Where(account => account.UserId == userId)
            .Include(account => account.Cards)
            .SelectMany(account => account.Cards)
            .ToList();
    }
}