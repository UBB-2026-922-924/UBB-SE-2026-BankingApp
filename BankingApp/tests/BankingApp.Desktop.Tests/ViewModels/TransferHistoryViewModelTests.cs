namespace BankingApp.Desktop.Tests.ViewModels;

using System;
using System.Collections.Generic;
using BankingApp.Desktop.ViewModels;
using Contracts.Features.Transfers.Dtos;
using Contracts.Features.Transfers.Services;
using Domain.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

public class TransferHistoryViewModelTests
{
    private readonly Mock<ITransferService> _transferServiceMock = new();

    private TransferHistoryViewModel CreateViewModel() =>
        new(_transferServiceMock.Object, NullLogger<TransferHistoryViewModel>.Instance);

    [Fact]
    public async Task LoadHistoryAsync_WhenTransferHasReference_SetsReferenceDisplay()
    {
        // Arrange
        _transferServiceMock
            .Setup(service => service.GetHistoryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TransferResponse> { CreateTransferResponse(reference: "Invoice 42") });

        TransferHistoryViewModel vm = CreateViewModel();

        // Act
        await vm.LoadHistoryAsync();

        // Assert
        vm.Transfers.Should().HaveCount(1);
        vm.Transfers[0].ReferenceDisplay.Should().Be("Invoice 42");
    }

    [Fact]
    public async Task LoadHistoryAsync_WhenTransferReferenceIsNull_SetsReferenceDisplayToFallback()
    {
        // Arrange
        _transferServiceMock
            .Setup(service => service.GetHistoryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TransferResponse> { CreateTransferResponse(reference: null) });

        TransferHistoryViewModel vm = CreateViewModel();

        // Act
        await vm.LoadHistoryAsync();

        // Assert
        vm.Transfers[0].ReferenceDisplay.Should().Be("—");
    }

    [Fact]
    public async Task LoadHistoryAsync_WhenTransferReferenceIsEmpty_SetsReferenceDisplayToFallback()
    {
        // Arrange
        _transferServiceMock
            .Setup(service => service.GetHistoryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TransferResponse> { CreateTransferResponse(reference: "") });

        TransferHistoryViewModel vm = CreateViewModel();

        // Act
        await vm.LoadHistoryAsync();

        // Assert
        vm.Transfers[0].ReferenceDisplay.Should().Be("—");
    }

    [Fact]
    public async Task LoadHistoryAsync_MapsAmountCurrencyAndStatusCorrectly()
    {
        // Arrange
        _transferServiceMock
            .Setup(service => service.GetHistoryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TransferResponse>
            {
                CreateTransferResponse(amount: 250.5m, currency: "EUR", status: TransferStatus.Completed)
            });

        TransferHistoryViewModel vm = CreateViewModel();

        // Act
        await vm.LoadHistoryAsync();

        // Assert
        TransferHistoryDisplayItem item = vm.Transfers[0];
        item.Amount.Should().Be("-250.50");
        item.Currency.Should().Be("EUR");
        item.StatusDisplay.Should().Be("Completed");
    }

    [Fact]
    public async Task LoadHistoryAsync_WhenResultIsEmpty_HasNoTransfersIsTrueAndShowTransferListIsFalse()
    {
        // Arrange
        _transferServiceMock
            .Setup(service => service.GetHistoryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TransferResponse>());

        TransferHistoryViewModel vm = CreateViewModel();

        // Act
        await vm.LoadHistoryAsync();

        // Assert
        vm.Transfers.Should().BeEmpty();
        vm.HasNoTransfers.Should().BeTrue();
        vm.ShowTransferList.Should().BeFalse();
        vm.HasError.Should().BeFalse();
    }

    [Fact]
    public async Task LoadHistoryAsync_WhenApiFails_HasErrorIsTrueAndShowTransferListIsFalse()
    {
        // Arrange
        _transferServiceMock
            .Setup(service => service.GetHistoryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Failure());

        TransferHistoryViewModel vm = CreateViewModel();

        // Act
        await vm.LoadHistoryAsync();

        // Assert
        vm.HasError.Should().BeTrue();
        vm.ShowTransferList.Should().BeFalse();
        vm.HasNoTransfers.Should().BeFalse();
    }

    private static TransferResponse CreateTransferResponse(
        string? reference = null,
        decimal amount = 100m,
        string currency = "RON",
        TransferStatus status = TransferStatus.Completed) =>
        new()
        {
            RecipientName = "Jane Doe",
            RecipientIban = "RO12BANK1234567890123456",
            Amount = amount,
            Currency = currency,
            Fee = 1m,
            Reference = reference,
            Status = status,
            CreatedAt = new DateTime(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc),
        };
}
