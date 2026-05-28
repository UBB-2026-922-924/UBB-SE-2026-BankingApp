namespace BankingApp.Application.Features.Cards.Services;

using System.Globalization;
using System.Security.Cryptography;
using Contracts.Features.Cards.Dtos;
using Domain.Aggregates.AccountAggregate;
using Domain.Aggregates.AccountAggregate.Entities;
using Domain.Aggregates.UserAggregate;
using Domain.Common.Errors;
using Domain.Enums;
using Domain.Repositories;
using Domain.ValueObjects;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Shared.Clock;
using Shared.Persistence;
using ApplicationLogMessages = Common.Logging.ApplicationLogMessages;
using Currency = NodaMoney.Currency;

public sealed class CardService(
    IAccountRepository accountRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ISystemClock clock,
    ILogger<CardService> logger)
    : ICardService
{
    private const int CardNumberLength = 16;
    private const int CvvLength = 3;
    private const int ExpiryYears = 4;
    private const string DefaultCurrency = "USD";
    private const string IbanCountryCode = "RO";
    private const int IbanCheckDigits = 49;
    private const int IbanBankCode = 12345678;
    private const int IbanAccountNumberDigits = 10;
    private const string IbanBban = "BANK";
    private const int DecimalBase = 10;
    private const int IbanAccountNumberUpperBound = 1_000_000_000;

    public async Task<ErrorOr<List<CardDetailsDto>>> GetCardsAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            IReadOnlyCollection<Account> accounts = await accountRepository.ListByUserIdAsync(userId, cancellationToken);

            return (from account in accounts
                from card in account.Cards
                select new CardDetailsDto
                {
                    Id = card.Id,
                    CardNumber = card.GetMaskedNumber(),
                    FullCardNumber = card.CardNumber,
                    SecurityCode = card.Cvv,
                    CardholderName = card.CardholderName,
                    ExpiryDate = card.ExpiryDate,
                    CardType = card.CardType,
                    CardBrand = card.CardBrand,
                    Status = card.Status,
                    IsContactlessEnabled = card.IsContactlessEnabled,
                    IsOnlineEnabled = card.IsOnlineEnabled,
                    AccountName = account.AccountName,
                    AccountIban = account.Iban.Value,
                    AccountId = account.Id,
                    AccountBalance = account.Balance.Amount,
                    AccountCurrency = account.Balance.Currency.Code
                }).ToList();
        }
        catch (Exception exception)
        {
            ApplicationLogMessages.GetCardsQueryFailed(logger, userId, exception.Message);
            return UserErrors.NotFound;
        }
    }

    public async Task<ErrorOr<CardDetailsDto>> IssueAsync(int userId, CardType cardType, string? cardBrand, CancellationToken cancellationToken = default)
    {
        User? user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            ApplicationLogMessages.IssueCardAccountNotFound(logger, 0, userId);
            return UserErrors.NotFound;
        }

        string ibanValue = GenerateIban();
        ErrorOr<Iban> ibanResult = Iban.Create(ibanValue);
        if (ibanResult.IsError)
        {
            return ibanResult.FirstError;
        }

        AccountType accountType = cardType == CardType.Credit
            ? AccountType.Credit
            : AccountType.Checking;

        DateTime now = clock.UtcNow;
        var account = Account.Open(userId, ibanResult.Value, Currency.FromCode(DefaultCurrency), accountType, null, now);

        await accountRepository.AddAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        string cardNumber = GenerateCardNumber();
        string cvv = GenerateCvv();
        DateTime expiryDate = now.AddYears(ExpiryYears);
        string cardholderName = user.FullName.ToUpperInvariant();

        Card card = account.IssueCard(cardNumber, cardholderName, expiryDate, cvv, cardType, cardBrand, now);

        await accountRepository.UpdateAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ApplicationLogMessages.CardIssued(logger, card.Id, account.Id, userId);

        return new CardDetailsDto
        {
            Id = card.Id,
            CardNumber = card.GetMaskedNumber(),
            FullCardNumber = card.CardNumber,
            SecurityCode = card.Cvv,
            CardholderName = card.CardholderName,
            ExpiryDate = card.ExpiryDate,
            CardType = card.CardType,
            CardBrand = card.CardBrand,
            Status = card.Status,
            IsContactlessEnabled = card.IsContactlessEnabled,
            IsOnlineEnabled = card.IsOnlineEnabled,
            AccountId = account.Id,
            AccountName = account.AccountName
        };
    }

    public async Task<ErrorOr<Success>> FreezeAsync(int userId, int cardId, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Account> accounts = await accountRepository.ListByUserIdAsync(userId, cancellationToken);

        Card? card = accounts
            .SelectMany(account => account.Cards)
            .FirstOrDefault(card => card.Id == cardId && card.UserId == userId);

        if (card is null)
        {
            ApplicationLogMessages.CardNotFound(logger, cardId, userId);
            return CardErrors.NotFound;
        }

        if (card.Status == CardStatus.Frozen)
        {
            return CardErrors.AlreadyFrozen;
        }

        if (card.Status == CardStatus.Cancelled)
        {
            return CardErrors.AlreadyCancelled;
        }

        Account account = accounts.First(a => a.Cards.Any(c => c.Id == cardId));
        card.Freeze();
        await accountRepository.UpdateAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ApplicationLogMessages.CardFrozen(logger, cardId, userId);
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> UnfreezeAsync(int userId, int cardId, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Account> accounts = await accountRepository.ListByUserIdAsync(userId, cancellationToken);

        Card? card = accounts
            .SelectMany(account => account.Cards)
            .FirstOrDefault(card => card.Id == cardId && card.UserId == userId);

        if (card is null)
        {
            ApplicationLogMessages.CardNotFound(logger, cardId, userId);
            return CardErrors.NotFound;
        }

        if (card.Status == CardStatus.Cancelled)
        {
            return CardErrors.AlreadyCancelled;
        }

        if (card.Status != CardStatus.Frozen)
        {
            return CardErrors.NotFrozen;
        }

        Account account = accounts.First(a => a.Cards.Any(c => c.Id == cardId));
        card.Unfreeze();
        await accountRepository.UpdateAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ApplicationLogMessages.CardUnfrozen(logger, cardId, userId);
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> CancelAsync(int userId, int cardId, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Account> accounts = await accountRepository.ListByUserIdAsync(userId, cancellationToken);

        Card? card = accounts
            .SelectMany(account => account.Cards)
            .FirstOrDefault(card => card.Id == cardId && card.UserId == userId);

        if (card is null)
        {
            ApplicationLogMessages.CardNotFound(logger, cardId, userId);
            return CardErrors.NotFound;
        }

        if (card.Status == CardStatus.Cancelled)
        {
            return CardErrors.AlreadyCancelled;
        }

        Account account = accounts.First(a => a.Cards.Any(c => c.Id == cardId));
        card.Cancel(clock.UtcNow);
        await accountRepository.UpdateAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ApplicationLogMessages.CardCancelled(logger, cardId, userId);
        return Result.Success;
    }

    private static string GenerateCardNumber()
    {
        Span<byte> bytes = stackalloc byte[CardNumberLength];
        RandomNumberGenerator.Fill(bytes);
        var sb = new System.Text.StringBuilder(CardNumberLength);
        foreach (byte b in bytes)
        {
            sb.Append(b % DecimalBase);
        }

        return sb.ToString();
    }

    private static string GenerateCvv()
    {
        Span<byte> bytes = stackalloc byte[CvvLength];
        RandomNumberGenerator.Fill(bytes);
        var sb = new System.Text.StringBuilder(CvvLength);
        foreach (byte b in bytes)
        {
            sb.Append(b % DecimalBase);
        }

        return sb.ToString();
    }

    private static string GenerateIban()
    {
        int accountSuffix = RandomNumberGenerator.GetInt32(IbanAccountNumberUpperBound);
        string bban = $"{IbanBban}{IbanBankCode:D8}{accountSuffix.ToString(CultureInfo.InvariantCulture).PadLeft(IbanAccountNumberDigits, '0')}";
        return $"{IbanCountryCode}{IbanCheckDigits:D2}{bban}";
    }
}
