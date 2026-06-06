namespace BankingApp.Application.Tests.Features.Transactions.Services;

using Application.Features.Transactions.Services;
using BankingApp.Contracts.Features.Transactions.Dtos;
using ErrorOr;

public sealed class TransactionServiceTests
{
    private const int UserId = 1;
    private const int TransactionId = 42;
    private const string ExportFormat = "pdf";

    private readonly Mock<ITransactionHistoryRepository> _repositoryMock = new();
    private readonly Mock<ITransactionExportService> _exportServiceMock = new();

    [Fact]
    public async Task GetFilterMetadataAsync_WhenCalled_ShouldReturnDistinctSortedFilterOptions()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        _repositoryMock
            .Setup(repository => repository.GetTransactionsByUserId(UserId))
            .Returns(
            [
                BuildTransaction(id: 1, type: "TRANSFER", status: "Completed", direction: "Out"),
                BuildTransaction(id: 2, type: "BILL_PAYMENT", status: "Completed", direction: "Out"),
                BuildTransaction(id: 3, type: "TRANSFER", status: "Failed", direction: "In"),
            ]);

        _repositoryMock
            .Setup(repository => repository.GetAccountsByUserId(UserId))
            .Returns([]);

        _repositoryMock
            .Setup(repository => repository.GetCardsByUserId(UserId))
            .Returns([]);

        TransactionService service = CreateService();

        ErrorOr<TransactionFilterMetadataResponse> result =
            await service.GetFilterMetadataAsync(UserId, cancellationToken);

        result.IsError.Should().BeFalse();
        result.Value.Success.Should().BeTrue();
        result.Value.AvailableTransactionTypes.Should().BeEquivalentTo(["BILL_PAYMENT", "TRANSFER"]);
        result.Value.AvailableStatuses.Should().BeEquivalentTo(["Completed", "Failed"]);
        result.Value.AvailableDirections.Should().BeEquivalentTo(["In", "Out"]);
    }

    [Fact]
    public async Task GetHistoryAsync_WhenSearchTermProvided_ShouldFilterByCounterpartyOrMerchantOrReference()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        _repositoryMock
            .Setup(repository => repository.GetTransactionsByUserId(UserId))
            .Returns(
            [
                BuildTransaction(id: 1, counterparty: "Alice", reference: "REF-001"),
                BuildTransaction(id: 2, counterparty: "Bob", reference: "REF-002"),
                BuildTransaction(id: 3, counterparty: "Charlie", reference: "REF-001"),
            ]);

        TransactionService service = CreateService();
        TransactionHistoryRequest request = new() { SearchTerm = "alice" };

        ErrorOr<TransactionHistoryResponse> result =
            await service.GetHistoryAsync(UserId, request, cancellationToken);

        result.IsError.Should().BeFalse();
        result.Value.Transactions.Should().ContainSingle(transaction => transaction.Id == 1);
    }

    [Fact]
    public async Task GetHistoryAsync_WhenAmountRangeProvided_ShouldFilterByMinAndMaxAmount()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        _repositoryMock
            .Setup(repository => repository.GetTransactionsByUserId(UserId))
            .Returns(
            [
                BuildTransaction(id: 1, amount: 50m),
                BuildTransaction(id: 2, amount: 150m),
                BuildTransaction(id: 3, amount: 300m),
            ]);

        TransactionService service = CreateService();
        TransactionHistoryRequest request = new() { MinimumAmount = 100m, MaximumAmount = 200m };

        ErrorOr<TransactionHistoryResponse> result =
            await service.GetHistoryAsync(UserId, request, cancellationToken);

        result.IsError.Should().BeFalse();
        result.Value.Transactions.Should().ContainSingle(transaction => transaction.Id == 2);
    }

    [Fact]
    public async Task GetHistoryAsync_WhenSortedByAmountAscending_ShouldReturnTransactionsInCorrectOrder()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        _repositoryMock
            .Setup(repository => repository.GetTransactionsByUserId(UserId))
            .Returns(
            [
                BuildTransaction(id: 1, amount: 300m),
                BuildTransaction(id: 2, amount: 100m),
                BuildTransaction(id: 3, amount: 200m),
            ]);

        TransactionService service = CreateService();
        TransactionHistoryRequest request = new()
        {
            SortField = TransactionSortFields.Amount,
            SortDirection = SortDirections.Asc
        };

        ErrorOr<TransactionHistoryResponse> result =
            await service.GetHistoryAsync(UserId, request, cancellationToken);

        result.IsError.Should().BeFalse();
        result.Value.Transactions.Select(transaction => transaction.Id)
            .Should().ContainInOrder(2, 3, 1);
    }

    [Fact]
    public async Task GetTransactionByIdAsync_WhenTransactionExists_ShouldReturnTransactionDetails()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        _repositoryMock
            .Setup(repository => repository.GetTransactionById(UserId, TransactionId))
            .Returns(BuildTransaction(id: TransactionId));

        TransactionService service = CreateService();

        ErrorOr<TransactionDetailsResponse> result =
            await service.GetTransactionByIdAsync(UserId, TransactionId, cancellationToken);

        result.IsError.Should().BeFalse();
        result.Value.Success.Should().BeTrue();
        result.Value.Transaction!.Id.Should().Be(TransactionId);
    }

    [Fact]
    public async Task GetTransactionByIdAsync_WhenTransactionDoesNotExist_ShouldReturnNotFoundError()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        _repositoryMock
            .Setup(repository => repository.GetTransactionById(UserId, TransactionId))
            .Returns((TransactionHistoryItemDto?)null);

        TransactionService service = CreateService();

        ErrorOr<TransactionDetailsResponse> result =
            await service.GetTransactionByIdAsync(UserId, TransactionId, cancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task ExportTransactionsAsync_WhenCalled_ShouldApplyFiltersAndDelegateToExportService()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        List<TransactionHistoryItemDto> transactions =
        [
            BuildTransaction(id: 1, direction: "Out"),
            BuildTransaction(id: 2, direction: "In"),
        ];

        _repositoryMock
            .Setup(repository => repository.GetTransactionsByUserId(UserId))
            .Returns(transactions);

        TransactionExportResult expectedResult = new()
        {
            Content = [0x25, 0x50, 0x44, 0x46],
            ContentType = "application/pdf",
            FileName = "statement.pdf"
        };

        _exportServiceMock
            .Setup(service => service.ExportStatement(
                It.IsAny<IReadOnlyCollection<TransactionHistoryItemDto>>(),
                It.IsAny<TransactionHistoryRequest>(),
                ExportFormat))
            .Returns(expectedResult);

        TransactionService service = CreateService();
        TransactionExportRequest request = new() { Direction = "Out", Format = ExportFormat };

        ErrorOr<TransactionExportResult> result =
            await service.ExportTransactionsAsync(UserId, request, cancellationToken);

        result.IsError.Should().BeFalse();
        result.Value.FileName.Should().Be("statement.pdf");
        _exportServiceMock.Verify(
            exportService => exportService.ExportStatement(
                It.Is<IReadOnlyCollection<TransactionHistoryItemDto>>(list => list.All(t => t.Direction == "Out")),
                It.IsAny<TransactionHistoryRequest>(),
                ExportFormat),
            Times.Once);
    }

    [Fact]
    public async Task ExportReceiptAsync_WhenTransactionDoesNotExist_ShouldReturnNotFoundError()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        _repositoryMock
            .Setup(repository => repository.GetTransactionById(UserId, TransactionId))
            .Returns((TransactionHistoryItemDto?)null);

        TransactionService service = CreateService();

        ErrorOr<TransactionExportResult> result =
            await service.ExportReceiptAsync(UserId, TransactionId, cancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _exportServiceMock.Verify(
            exportService => exportService.ExportReceipt(It.IsAny<TransactionHistoryItemDto>()),
            Times.Never);
    }

    [Fact]
    public async Task ExportReceiptAsync_WhenTransactionExists_ShouldDelegateToExportService()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        TransactionHistoryItemDto transaction = BuildTransaction(id: TransactionId);

        _repositoryMock
            .Setup(repository => repository.GetTransactionById(UserId, TransactionId))
            .Returns(transaction);

        TransactionExportResult expectedResult = new()
        {
            Content = [0x25, 0x50, 0x44, 0x46],
            ContentType = "application/pdf",
            FileName = "receipt.pdf"
        };

        _exportServiceMock
            .Setup(service => service.ExportReceipt(transaction))
            .Returns(expectedResult);

        TransactionService service = CreateService();

        ErrorOr<TransactionExportResult> result =
            await service.ExportReceiptAsync(UserId, TransactionId, cancellationToken);

        result.IsError.Should().BeFalse();
        result.Value.FileName.Should().Be("receipt.pdf");
        _exportServiceMock.Verify(
            exportService => exportService.ExportReceipt(transaction),
            Times.Once);
    }

    private TransactionService CreateService() =>
        new(_repositoryMock.Object, _exportServiceMock.Object);

    private static TransactionHistoryItemDto BuildTransaction(
        int id = 1,
        string type = "TRANSFER",
        string status = "Completed",
        string direction = "Out",
        string counterparty = "Test Counterparty",
        string reference = "REF-001",
        decimal amount = 100m)
    {
        return new TransactionHistoryItemDto
        {
            Id = id,
            AccountId = 1,
            TransactionType = type,
            Status = status,
            Direction = direction,
            CounterpartyOrMerchant = counterparty,
            ReferenceNumber = reference,
            Amount = amount,
            Currency = "USD",
            Timestamp = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            AccountName = "Test Account",
            AccountIban = "RO49BANK123456780000000001"
        };
    }
}