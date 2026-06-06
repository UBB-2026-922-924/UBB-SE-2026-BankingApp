namespace BankingApp.Application.Features.Transactions.Services;

using Contracts.Features.Transactions.Dtos;
using Domain.Aggregates.AccountAggregate;
using Domain.Aggregates.AccountAggregate.Entities;
using ErrorOr;

public sealed class TransactionService(
    ITransactionHistoryRepository transactionHistoryRepository,
    ITransactionExportService? transactionExportService = null)
    : ITransactionService
{
    private const string FiltersLoadedMessage = "Transaction filters loaded successfully.";
    private const string HistoryLoadedMessage = "Transaction history loaded successfully.";
    private const string DetailsLoadedMessage = "Transaction details loaded successfully.";
    private const string NotFoundMessage = "Transaction not found.";
    private const int CardNumberMaskMinLength = 4;

    public Task<ErrorOr<TransactionFilterMetadataResponse>> GetFilterMetadataAsync(
        int userId, CancellationToken cancellationToken = default)
    {
        List<TransactionHistoryItemDto> transactions = transactionHistoryRepository.GetTransactionsByUserId(userId);
        List<Account> accounts = transactionHistoryRepository.GetAccountsByUserId(userId);
        List<Card> cards = transactionHistoryRepository.GetCardsByUserId(userId);

        TransactionFilterMetadataResponse response = new()
        {
            Success = true,
            Message = FiltersLoadedMessage,
            Accounts = accounts
                .Select(account => new AccountFilterOptionDto
                {
                    Id = account.Id,
                    Name = account.AccountName ?? $"Account {account.Id}",
                    Iban = account.Iban.Value
                })
                .OrderBy(account => account.Name, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            Cards = cards
                .Select(card => new CardFilterOptionDto
                {
                    Id = card.Id,
                    Label = $"{(card.CardBrand ?? card.CardType.ToString())} {MaskCardNumber(card.CardNumber)}"
                })
                .OrderBy(card => card.Label, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            AvailableTransactionTypes = transactions
                .Select(transaction => transaction.TransactionType)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            AvailableStatuses = transactions
                .Select(transaction => transaction.Status)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            AvailableDirections = transactions
                .Select(transaction => transaction.Direction)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList()
        };

        return Task.FromResult<ErrorOr<TransactionFilterMetadataResponse>>(response);
    }

    public Task<ErrorOr<TransactionHistoryResponse>> GetHistoryAsync(
        int userId, TransactionHistoryRequest request, CancellationToken cancellationToken = default)
    {
        TransactionHistoryRequest normalizedRequest = NormalizeRequest(request);
        List<TransactionHistoryItemDto> transactions = transactionHistoryRepository.GetTransactionsByUserId(userId);
        List<TransactionHistoryItemDto> filtered = ApplyFiltersAndSort(transactions, normalizedRequest);

        TransactionHistoryResponse response = new()
        {
            Success = true,
            Message = HistoryLoadedMessage,
            AppliedFilters = normalizedRequest,
            Transactions = filtered
        };

        return Task.FromResult<ErrorOr<TransactionHistoryResponse>>(response);
    }

    public Task<ErrorOr<TransactionDetailsResponse>> GetTransactionByIdAsync(
        int userId, int transactionId, CancellationToken cancellationToken = default)
    {
        TransactionHistoryItemDto? transaction = transactionHistoryRepository.GetTransactionById(userId, transactionId);

        TransactionDetailsResponse response = transaction is null
            ? new TransactionDetailsResponse { Success = false, Message = NotFoundMessage }
            : new TransactionDetailsResponse { Success = true, Message = DetailsLoadedMessage, Transaction = transaction };

        if (!response.Success)
        {
            return Task.FromResult<ErrorOr<TransactionDetailsResponse>>(
                Error.NotFound("Transaction.NotFound", NotFoundMessage));
        }

        return Task.FromResult<ErrorOr<TransactionDetailsResponse>>(response);
    }

    public Task<ErrorOr<TransactionExportResult>> ExportTransactionsAsync(
        int userId, TransactionExportRequest request, CancellationToken cancellationToken = default)
    {
        if (transactionExportService is null)
        {
            return Task.FromResult<ErrorOr<TransactionExportResult>>(
                Error.Failure("Export.NotAvailable", "Export functionality is not yet available."));
        }

        TransactionHistoryRequest normalizedRequest = NormalizeRequest(request);
        List<TransactionHistoryItemDto> transactions = transactionHistoryRepository.GetTransactionsByUserId(userId);
        List<TransactionHistoryItemDto> filtered = ApplyFiltersAndSort(transactions, normalizedRequest);
        TransactionExportResult result = transactionExportService.ExportStatement(filtered, normalizedRequest, request.Format);
        return Task.FromResult<ErrorOr<TransactionExportResult>>(result);
    }

    public Task<ErrorOr<TransactionExportResult>> ExportReceiptAsync(
        int userId, int transactionId, CancellationToken cancellationToken = default)
    {
        if (transactionExportService is null)
        {
            return Task.FromResult<ErrorOr<TransactionExportResult>>(
                Error.Failure("Export.NotAvailable", "Export functionality is not yet available."));
        }

        TransactionHistoryItemDto? transaction = transactionHistoryRepository.GetTransactionById(userId, transactionId);
        if (transaction is null)
        {
            return Task.FromResult<ErrorOr<TransactionExportResult>>(
                Error.NotFound("Transaction.NotFound", NotFoundMessage));
        }

        TransactionExportResult result = transactionExportService.ExportReceipt(transaction);
        return Task.FromResult<ErrorOr<TransactionExportResult>>(result);
    }

    private static List<TransactionHistoryItemDto> ApplyFiltersAndSort(
        IEnumerable<TransactionHistoryItemDto> transactions,
        TransactionHistoryRequest request)
    {
        IEnumerable<TransactionHistoryItemDto> query = transactions;

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(transaction =>
                ContainsInsensitive(transaction.CounterpartyOrMerchant, request.SearchTerm) ||
                ContainsInsensitive(transaction.ReferenceNumber, request.SearchTerm) ||
                ContainsInsensitive(transaction.Description, request.SearchTerm));
        }

        if (request.FromDate.HasValue)
        {
            DateTime fromDate = request.FromDate.Value.Date;
            query = query.Where(transaction => transaction.Timestamp.Date >= fromDate);
        }

        if (request.ToDate.HasValue)
        {
            DateTime toDate = request.ToDate.Value.Date;
            query = query.Where(transaction => transaction.Timestamp.Date <= toDate);
        }

        if (!string.IsNullOrWhiteSpace(request.TransactionType))
        {
            query = query.Where(transaction =>
                string.Equals(transaction.TransactionType, request.TransactionType, StringComparison.OrdinalIgnoreCase));
        }

        if (request.MinimumAmount.HasValue)
        {
            query = query.Where(transaction => transaction.Amount >= request.MinimumAmount.Value);
        }

        if (request.MaximumAmount.HasValue)
        {
            query = query.Where(transaction => transaction.Amount <= request.MaximumAmount.Value);
        }

        if (request.AccountId.HasValue)
        {
            query = query.Where(transaction => transaction.AccountId == request.AccountId.Value);
        }

        if (request.CardId.HasValue)
        {
            query = query.Where(transaction => transaction.CardId == request.CardId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(transaction =>
                string.Equals(transaction.Status, request.Status, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(request.Direction))
        {
            query = query.Where(transaction =>
                string.Equals(transaction.Direction, request.Direction, StringComparison.OrdinalIgnoreCase));
        }

        bool sortAscending = string.Equals(request.SortDirection, SortDirections.Asc, StringComparison.OrdinalIgnoreCase);

        query = string.Equals(request.SortField, TransactionSortFields.Amount, StringComparison.OrdinalIgnoreCase)
            ? (sortAscending
                ? query.OrderBy(transaction => transaction.Amount).ThenBy(transaction => transaction.Timestamp)
                : query.OrderByDescending(transaction => transaction.Amount).ThenByDescending(transaction => transaction.Timestamp))
            : (sortAscending
                ? query.OrderBy(transaction => transaction.Timestamp).ThenBy(transaction => transaction.Id)
                : query.OrderByDescending(transaction => transaction.Timestamp).ThenByDescending(transaction => transaction.Id));

        return query.ToList();
    }

    private static TransactionHistoryRequest NormalizeRequest(TransactionHistoryRequest request)
    {
        return new TransactionHistoryRequest
        {
            SearchTerm = request.SearchTerm?.Trim(),
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            TransactionType = NormalizeOptionalValue(request.TransactionType),
            MinimumAmount = request.MinimumAmount,
            MaximumAmount = request.MaximumAmount,
            AccountId = request.AccountId,
            CardId = request.CardId,
            Status = NormalizeOptionalValue(request.Status),
            Direction = NormalizeOptionalValue(request.Direction),
            SortField = NormalizeSortField(request.SortField),
            SortDirection = NormalizeSortDirection(request.SortDirection)
        };
    }

    private static TransactionHistoryRequest NormalizeRequest(TransactionExportRequest request)
    {
        return new TransactionHistoryRequest
        {
            SearchTerm = request.SearchTerm?.Trim(),
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            TransactionType = NormalizeOptionalValue(request.TransactionType),
            MinimumAmount = request.MinimumAmount,
            MaximumAmount = request.MaximumAmount,
            AccountId = request.AccountId,
            CardId = request.CardId,
            Status = NormalizeOptionalValue(request.Status),
            Direction = NormalizeOptionalValue(request.Direction),
            SortField = NormalizeSortField(request.SortField),
            SortDirection = NormalizeSortDirection(request.SortDirection)
        };
    }

    private static string NormalizeSortField(string? sortField)
    {
        return string.Equals(sortField, TransactionSortFields.Amount, StringComparison.OrdinalIgnoreCase)
            ? TransactionSortFields.Amount
            : TransactionSortFields.Date;
    }

    private static string NormalizeSortDirection(string? sortDirection)
    {
        return string.Equals(sortDirection, SortDirections.Asc, StringComparison.OrdinalIgnoreCase)
            ? SortDirections.Asc
            : SortDirections.Desc;
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool ContainsInsensitive(string? source, string searchTerm)
    {
        return !string.IsNullOrWhiteSpace(source) &&
               source.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
    }

    private static string MaskCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < CardNumberMaskMinLength)
        {
            return "****";
        }

        return $"**** {cardNumber[^CardNumberMaskMinLength..]}";
    }
}