namespace BankingApp.Application.Features.Savings.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BankingApp.Domain.Aggregates.SavingsAggregate;
using BankingApp.Domain.Aggregates.SavingsAggregate.Entities;
using BankingApp.Domain.Common.Errors;
using BankingApp.Domain.Enums;
using BankingApp.Domain.Repositories;
using ErrorOr;
using Shared.Persistence;

public sealed class SavingsService(
    ISavingsRepository savingsRepository,
    IUnitOfWork unitOfWork)
    : ISavingsService
{
    private const int MaxActiveAccounts = 5;
    private const decimal FixedDepositApy = 0.04m;
    private const decimal GoalSavingsApy = 0.03m;
    private const decimal HighYieldApy = 0.03m;
    private const decimal DefaultApy = 0.02m;
    private const decimal EarlyClosurePenaltyRate = 0.02m;
    private const decimal EarlyWithdrawalPenaltyRate = 0.02m;

    public async Task<ErrorOr<SavingsAccount>> CreateAccountAsync(
        int userId,
        SavingsType savingsType,
        string? accountName,
        int? fundingAccountId,
        decimal initialDeposit,
        decimal? targetAmount,
        DateTime? targetDate,
        DateTime? maturityDate,
        DepositFrequency? depositFrequency,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<SavingsAccount> active = await savingsRepository.GetSavingsAccountsByUserIdAsync(userId, false, cancellationToken);
        if (active.Count >= MaxActiveAccounts)
        {
            return Error.Validation("Savings.MaxAccountsReached", $"Cannot have more than {MaxActiveAccounts} active savings accounts.");
        }

        if (savingsType == SavingsType.GoalSavings)
        {
            if (!targetDate.HasValue || targetDate.Value.Date <= DateTime.Today)
            {
                return Error.Validation("Savings.InvalidTargetDate", "GoalSavings accounts require a future target date.");
            }

            if (!targetAmount.HasValue || targetAmount.Value <= 0)
            {
                return Error.Validation("Savings.InvalidTargetAmount", "GoalSavings accounts require a positive target amount.");
            }
        }

        decimal apy = savingsType switch
        {
            SavingsType.FixedDeposit => FixedDepositApy,
            SavingsType.GoalSavings => GoalSavingsApy,
            SavingsType.HighYield => HighYieldApy,
            _ => DefaultApy,
        };

        var account = SavingsAccount.Create(userId, savingsType, apy, accountName, fundingAccountId, targetAmount, targetDate, maturityDate, DateTime.UtcNow);
        SavingsAccount created = await savingsRepository.CreateSavingsAccountAsync(account, cancellationToken);

        if (initialDeposit > 0)
        {
            await savingsRepository.DepositAsync(created.Id, initialDeposit, "Initial deposit", cancellationToken);
        }

        return created;
    }

    public async Task<ErrorOr<IReadOnlyCollection<SavingsAccount>>> GetAccountsAsync(int userId, bool includesClosed, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<SavingsAccount> accounts = await savingsRepository.GetSavingsAccountsByUserIdAsync(userId, includesClosed, cancellationToken);
        return ErrorOrFactory.From(accounts);
    }

    public async Task<ErrorOr<(decimal NewBalance, int TransactionId, DateTime Timestamp)>> DepositAsync(
        int userId, int accountId, decimal amount, string source, CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            return SavingsErrors.InvalidDepositAmount;
        }

        IReadOnlyCollection<SavingsAccount> accounts = await savingsRepository.GetSavingsAccountsByUserIdAsync(userId, true, cancellationToken);
        if (!accounts.Any(a => a.Id == accountId))
        {
            return SavingsErrors.AccountNotFound;
        }

        (decimal newBalance, int transactionId, DateTime timestamp) = await savingsRepository.DepositAsync(accountId, amount, source, cancellationToken);
        return (newBalance, transactionId, timestamp);
    }

    public async Task<ErrorOr<(decimal AmountWithdrawn, decimal PenaltyApplied, decimal NewBalance)>> WithdrawAsync(
        int userId, int accountId, decimal amount, string destinationLabel, CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            return SavingsErrors.InvalidDepositAmount;
        }

        IReadOnlyCollection<SavingsAccount> accounts = await savingsRepository.GetSavingsAccountsByUserIdAsync(userId, true, cancellationToken);
        SavingsAccount? account = accounts.FirstOrDefault(a => a.Id == accountId);
        if (account is null)
        {
            return SavingsErrors.AccountNotFound;
        }

        if (account.Balance < amount)
        {
            return SavingsErrors.InsufficientBalance;
        }

        decimal penalty = IsEarlyWithdrawal(account) ? amount * EarlyWithdrawalPenaltyRate : 0m;
        decimal totalDebit = amount + penalty;

        if (totalDebit > account.Balance)
        {
            return SavingsErrors.InsufficientBalance;
        }

        (decimal withdrawn, decimal penaltyApplied, decimal newBalance, _) = await savingsRepository.WithdrawAsync(accountId, totalDebit, destinationLabel, penalty, cancellationToken);
        return (withdrawn, penaltyApplied, newBalance);
    }

    public async Task<ErrorOr<(decimal TransferredAmount, decimal PenaltyApplied)>> CloseAccountAsync(
        int userId, int accountId, int destinationAccountId, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<SavingsAccount> accounts = await savingsRepository.GetSavingsAccountsByUserIdAsync(userId, true, cancellationToken);
        SavingsAccount? closing = accounts.FirstOrDefault(a => a.Id == accountId);
        if (closing is null)
        {
            return SavingsErrors.AccountNotFound;
        }

        if (closing.AccountStatus == "Closed")
        {
            return SavingsErrors.AccountAlreadyClosed;
        }

        SavingsAccount? destination = accounts.FirstOrDefault(a => a.Id == destinationAccountId);
        if (destination is null || destination.AccountStatus == "Closed")
        {
            return SavingsErrors.AccountNotFound;
        }

        decimal penalty = IsEarlyWithdrawal(closing) ? closing.Balance * EarlyClosurePenaltyRate : 0m;
        decimal transferAmount = closing.Balance - penalty;

        (decimal transferred, decimal penaltyApplied, _) = await savingsRepository.CloseSavingsAccountAsync(
            accountId, destinationAccountId, transferAmount, penalty, cancellationToken);

        return (transferred, penaltyApplied);
    }

    public async Task<ErrorOr<AutoDeposit?>> GetAutoDepositAsync(int accountId, CancellationToken cancellationToken = default)
    {
        AutoDeposit? autoDeposit = await savingsRepository.GetAutoDepositAsync(accountId, cancellationToken);
        return autoDeposit;
    }

    public async Task<ErrorOr<Success>> SaveAutoDepositAsync(AutoDeposit autoDeposit, CancellationToken cancellationToken = default)
    {
        await savingsRepository.SaveAutoDepositAsync(autoDeposit, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }

    public async Task<ErrorOr<IReadOnlyCollection<(int Id, string DisplayName)>>> GetFundingSourcesAsync(int userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<(int Id, string DisplayName)> sources = await savingsRepository.GetFundingSourcesAsync(userId, cancellationToken);
        return ErrorOrFactory.From(sources);
    }

    public async Task<ErrorOr<(IReadOnlyCollection<SavingsTransaction> Items, int TotalCount)>> GetTransactionsAsync(
        int accountId, string typeFilter, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        (IReadOnlyCollection<SavingsTransaction> items, int totalCount) = await savingsRepository.GetTransactionsPagedAsync(accountId, typeFilter, page, pageSize, cancellationToken);
        return (items, totalCount);
    }

    private static bool IsEarlyWithdrawal(SavingsAccount account)
    {
        return account.SavingsType == SavingsType.FixedDeposit
            && account.MaturityDate.HasValue
            && account.MaturityDate.Value > DateTime.UtcNow;
    }
}
