namespace BankingApp.Application.Features.Savings.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.SavingsAggregate;
using Domain.Aggregates.SavingsAggregate.Entities;
using Domain.Enums;
using ErrorOr;

public interface ISavingsService
{
    public Task<ErrorOr<SavingsAccount>> CreateAccountAsync(
        int userId,
        SavingsType savingsType,
        string? accountName,
        int? fundingAccountId,
        decimal initialDeposit,
        decimal? targetAmount,
        DateTime? targetDate,
        DateTime? maturityDate,
        DepositFrequency? depositFrequency,
        CancellationToken cancellationToken = default);

    public Task<ErrorOr<IReadOnlyCollection<SavingsAccount>>> GetAccountsAsync(
        int userId,
        bool includesClosed,
        CancellationToken cancellationToken = default);

    public Task<ErrorOr<(decimal NewBalance, int TransactionId, DateTime Timestamp)>> DepositAsync(
        int userId,
        int accountId,
        decimal amount,
        string source,
        CancellationToken cancellationToken = default);

    public Task<ErrorOr<(decimal AmountWithdrawn, decimal PenaltyApplied, decimal NewBalance)>> WithdrawAsync(
        int userId,
        int accountId,
        decimal amount,
        string destinationLabel,
        CancellationToken cancellationToken = default);

    public Task<ErrorOr<(decimal TransferredAmount, decimal PenaltyApplied)>> CloseAccountAsync(
        int userId,
        int accountId,
        int destinationAccountId,
        CancellationToken cancellationToken = default);

    public Task<ErrorOr<AutoDeposit?>> GetAutoDepositAsync(int accountId, CancellationToken cancellationToken = default);

    public Task<ErrorOr<Success>> SaveAutoDepositAsync(AutoDeposit autoDeposit, CancellationToken cancellationToken = default);

    public Task<ErrorOr<IReadOnlyCollection<(int Id, string DisplayName)>>> GetFundingSourcesAsync(
        int userId,
        CancellationToken cancellationToken = default);

    public Task<ErrorOr<(IReadOnlyCollection<SavingsTransaction> Items, int TotalCount)>> GetTransactionsAsync(
        int accountId,
        string typeFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
