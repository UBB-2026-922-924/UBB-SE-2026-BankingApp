namespace BankingApp.Web.Tests.Controllers;

using BankingApp.Web.Controllers;
using BankingApp.Web.Models.Transfers;
using Contracts.Features.Beneficiaries.Dtos;
using Contracts.Features.Beneficiaries.Services;
using Contracts.Features.Transfers.Dtos;
using Contracts.Features.Transfers.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public sealed class TransfersControllerTests : IDisposable
{
    private readonly Mock<ITransferService> _transferServiceMock = new(MockBehavior.Strict);
    private readonly Mock<IBeneficiaryService> _beneficiaryServiceMock = new(MockBehavior.Strict);
    private readonly TransfersController _controller;

    public TransfersControllerTests()
    {
        DefaultHttpContext httpContext = new();
        _controller = new TransfersController(_transferServiceMock.Object, _beneficiaryServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
        };
    }

    public void Dispose() => _controller.Dispose();

    [Fact]
    public async Task New_WhenBeneficiaryIdIsMissing_ShouldReturnEmptyTransferForm()
    {
        IActionResult result = await _controller.New(null, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        TransferNewModel model = viewResult.Model.Should().BeOfType<TransferNewModel>().Subject;
        model.RecipientIban.Should().BeEmpty();
        _beneficiaryServiceMock.VerifyNoOtherCalls();
        _transferServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task New_WhenBeneficiaryServiceReturnsError_ShouldRedirectToBeneficiariesAndSetTempData()
    {
        _beneficiaryServiceMock
            .Setup(service => service.GetByIdAsync(9, CancellationToken.None))
            .ReturnsAsync(Error.NotFound("beneficiary.not_found", "Beneficiary was not found."));

        IActionResult result = await _controller.New(9, CancellationToken.None);

        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(BeneficiariesController.Index));
        redirect.ControllerName.Should().Be("Beneficiaries");
        _controller.TempData["Error"].Should().Be("Unable to open transfer for the selected beneficiary.");
        _beneficiaryServiceMock.VerifyAll();
        _transferServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task New_WhenValidationFails_ShouldRedirectToBeneficiariesAndSetTempData()
    {
        _beneficiaryServiceMock
            .Setup(service => service.GetByIdAsync(7, CancellationToken.None))
            .ReturnsAsync(new BeneficiaryDto
            {
                Id = 7,
                Name = "Jane Receiver",
                Iban = "RO49AAAA1B31007593840000",
                BankName = "Saved Bank"
            });

        _transferServiceMock
            .Setup(service => service.ValidateIbanAsync(
                It.Is<TransferIbanValidationRequest>(request => request.Iban == "RO49AAAA1B31007593840000"),
                CancellationToken.None))
            .ReturnsAsync(new TransferIbanValidationResponse
            {
                IsValid = false,
                BankName = string.Empty
            });

        _transferServiceMock
            .Setup(service => service.GetAccountsAsync(CancellationToken.None))
            .ReturnsAsync(new List<TransferAccountSelectionResponse>());

        IActionResult result = await _controller.New(7, CancellationToken.None);

        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(BeneficiariesController.Index));
        redirect.ControllerName.Should().Be("Beneficiaries");
        _controller.TempData["Error"].Should().Be("Unable to prepare a transfer for the selected beneficiary.");
        _beneficiaryServiceMock.VerifyAll();
        _transferServiceMock.VerifyAll();
    }

    [Fact]
    public async Task New_WhenAccountsLoadFails_ShouldRedirectToBeneficiariesAndSetTempData()
    {
        _beneficiaryServiceMock
            .Setup(service => service.GetByIdAsync(7, CancellationToken.None))
            .ReturnsAsync(new BeneficiaryDto
            {
                Id = 7,
                Name = "Jane Receiver",
                Iban = "RO49AAAA1B31007593840000",
                BankName = "Saved Bank"
            });

        _transferServiceMock
            .Setup(service => service.ValidateIbanAsync(
                It.Is<TransferIbanValidationRequest>(request => request.Iban == "RO49AAAA1B31007593840000"),
                CancellationToken.None))
            .ReturnsAsync(new TransferIbanValidationResponse
            {
                IsValid = true,
                BankName = "Validated Bank"
            });

        _transferServiceMock
            .Setup(service => service.GetAccountsAsync(CancellationToken.None))
            .ReturnsAsync(Error.Failure("transfers.accounts_failed", "Accounts could not be loaded."));

        IActionResult result = await _controller.New(7, CancellationToken.None);

        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(BeneficiariesController.Index));
        redirect.ControllerName.Should().Be("Beneficiaries");
        _controller.TempData["Error"].Should().Be("Unable to prepare a transfer for the selected beneficiary.");
        _beneficiaryServiceMock.VerifyAll();
        _transferServiceMock.VerifyAll();
    }

    [Fact]
    public async Task New_WhenBeneficiaryExists_ShouldReturnPrefilledValidatedTransferView()
    {
        List<TransferAccountSelectionResponse> transferAccounts =
        [
            new()
            {
                Id = 3,
                AccountName = "Daily Account",
                Iban = "RO11AAAA1B31007593840001",
                Currency = "RON",
                Balance = 1200.50m
            }
        ];

        _beneficiaryServiceMock
            .Setup(service => service.GetByIdAsync(7, CancellationToken.None))
            .ReturnsAsync(new BeneficiaryDto
            {
                Id = 7,
                Name = "Jane Receiver",
                Iban = "RO49AAAA1B31007593840000",
                BankName = "Saved Bank"
            });

        _transferServiceMock
            .Setup(service => service.ValidateIbanAsync(
                It.Is<TransferIbanValidationRequest>(request => request.Iban == "RO49AAAA1B31007593840000"),
                CancellationToken.None))
            .ReturnsAsync(new TransferIbanValidationResponse
            {
                IsValid = true,
                BankName = "Validated Bank"
            });

        _transferServiceMock
            .Setup(service => service.GetAccountsAsync(CancellationToken.None))
            .ReturnsAsync(transferAccounts);

        IActionResult result = await _controller.New(7, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("ValidateIban");
        TransferIbanValidatedModel model = viewResult.Model.Should().BeOfType<TransferIbanValidatedModel>().Subject;
        model.RecipientIban.Should().Be("RO49AAAA1B31007593840000");
        model.RecipientName.Should().Be("Jane Receiver");
        model.RecipientBankName.Should().Be("Saved Bank");
        model.Accounts.Should().ContainSingle();
        model.Accounts[0].Id.Should().Be(3);
        _beneficiaryServiceMock.VerifyAll();
        _transferServiceMock.VerifyAll();
    }

    [Fact]
    public async Task New_WhenBeneficiaryBankNameIsBlank_ShouldUseValidatedBankName()
    {
        _beneficiaryServiceMock
            .Setup(service => service.GetByIdAsync(7, CancellationToken.None))
            .ReturnsAsync(new BeneficiaryDto
            {
                Id = 7,
                Name = "Jane Receiver",
                Iban = "RO49AAAA1B31007593840000",
                BankName = " "
            });

        _transferServiceMock
            .Setup(service => service.ValidateIbanAsync(
                It.Is<TransferIbanValidationRequest>(request => request.Iban == "RO49AAAA1B31007593840000"),
                CancellationToken.None))
            .ReturnsAsync(new TransferIbanValidationResponse
            {
                IsValid = true,
                BankName = "Validated Bank"
            });

        _transferServiceMock
            .Setup(service => service.GetAccountsAsync(CancellationToken.None))
            .ReturnsAsync(new List<TransferAccountSelectionResponse>());

        IActionResult result = await _controller.New(7, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        TransferIbanValidatedModel model = viewResult.Model.Should().BeOfType<TransferIbanValidatedModel>().Subject;
        model.RecipientBankName.Should().Be("Validated Bank");
        _beneficiaryServiceMock.VerifyAll();
        _transferServiceMock.VerifyAll();
    }

    [Fact]
    public async Task ValidateIban_WhenModelStateIsInvalid_ShouldReturnNewViewWithoutCallingServices()
    {
        TransferNewModel model = new() { RecipientIban = string.Empty };
        _controller.ModelState.AddModelError(nameof(TransferNewModel.RecipientIban), "Recipient IBAN is required.");

        IActionResult result = await _controller.ValidateIban(model, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("New");
        viewResult.Model.Should().Be(model);
        _beneficiaryServiceMock.VerifyNoOtherCalls();
        _transferServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ValidateIban_WhenValidationFails_ShouldReturnNewViewWithModelError()
    {
        TransferNewModel model = new() { RecipientIban = "RO49AAAA1B31007593840000" };

        _transferServiceMock
            .Setup(service => service.ValidateIbanAsync(
                It.Is<TransferIbanValidationRequest>(request => request.Iban == model.RecipientIban),
                CancellationToken.None))
            .ReturnsAsync(Error.Validation("transfers.invalid_iban", "Invalid IBAN."));

        _transferServiceMock
            .Setup(service => service.GetAccountsAsync(CancellationToken.None))
            .ReturnsAsync(new List<TransferAccountSelectionResponse>());

        IActionResult result = await _controller.ValidateIban(model, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("New");
        viewResult.Model.Should().Be(model);
        _controller.ModelState[string.Empty]!.Errors
            .Should().ContainSingle(error => error.ErrorMessage == "The IBAN is invalid. Please check and try again.");
        _transferServiceMock.VerifyAll();
        _beneficiaryServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ValidateIban_WhenAccountsLoadFails_ShouldReturnNewViewAndSetTempData()
    {
        TransferNewModel model = new() { RecipientIban = "RO49AAAA1B31007593840000" };

        _transferServiceMock
            .Setup(service => service.ValidateIbanAsync(
                It.Is<TransferIbanValidationRequest>(request => request.Iban == model.RecipientIban),
                CancellationToken.None))
            .ReturnsAsync(new TransferIbanValidationResponse
            {
                IsValid = true,
                BankName = "Validated Bank"
            });

        _transferServiceMock
            .Setup(service => service.GetAccountsAsync(CancellationToken.None))
            .ReturnsAsync(Error.Failure("transfers.accounts_failed", "Accounts could not be loaded."));

        IActionResult result = await _controller.ValidateIban(model, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("New");
        viewResult.Model.Should().Be(model);
        _controller.TempData["Error"].Should().Be("Unable to load your accounts. Please try again.");
        _transferServiceMock.VerifyAll();
        _beneficiaryServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ValidateIban_WhenValidationSucceeds_ShouldReturnValidatedTransferView()
    {
        TransferNewModel model = new() { RecipientIban = "RO49AAAA1B31007593840000" };
        List<TransferAccountSelectionResponse> transferAccounts =
        [
            new()
            {
                Id = 5,
                AccountName = "Primary Account",
                Iban = "RO11AAAA1B31007593840001",
                Currency = "EUR",
                Balance = 800.25m
            }
        ];

        _transferServiceMock
            .Setup(service => service.ValidateIbanAsync(
                It.Is<TransferIbanValidationRequest>(request => request.Iban == model.RecipientIban),
                CancellationToken.None))
            .ReturnsAsync(new TransferIbanValidationResponse
            {
                IsValid = true,
                BankName = "Validated Bank"
            });

        _transferServiceMock
            .Setup(service => service.GetAccountsAsync(CancellationToken.None))
            .ReturnsAsync(transferAccounts);

        IActionResult result = await _controller.ValidateIban(model, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("ValidateIban");
        TransferIbanValidatedModel validatedModel = viewResult.Model.Should().BeOfType<TransferIbanValidatedModel>().Subject;
        validatedModel.RecipientIban.Should().Be(model.RecipientIban);
        validatedModel.RecipientBankName.Should().Be("Validated Bank");
        validatedModel.Accounts.Should().BeEquivalentTo(transferAccounts);
        _transferServiceMock.VerifyAll();
        _beneficiaryServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Preview_WhenModelStateIsInvalid_ShouldRepopulateAccountsAndReturnValidatedIbanView()
    {
        TransferIbanValidatedModel model = new()
        {
            RecipientName = "Jane Receiver",
            RecipientIban = "RO49AAAA1B31007593840000",
            RecipientBankName = "Validated Bank"
        };
        _controller.ModelState.AddModelError(nameof(TransferIbanValidatedModel.Amount), "Amount is required.");

        List<TransferAccountSelectionResponse> transferAccounts =
        [
            new()
            {
                Id = 3,
                AccountName = "Daily Account",
                Iban = "RO11AAAA1B31007593840001",
                Currency = "RON",
                Balance = 1200.50m
            }
        ];

        _transferServiceMock
            .Setup(service => service.GetAccountsAsync(CancellationToken.None))
            .ReturnsAsync(transferAccounts);

        IActionResult result = await _controller.Preview(model, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("ValidateIban");
        viewResult.Model.Should().Be(model);
        model.Accounts.Should().BeEquivalentTo(transferAccounts);
        _transferServiceMock.VerifyAll();
        _beneficiaryServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Preview_WhenAccountsLoadFails_ShouldReturnPreviewWithEmptySourceAccountData()
    {
        TransferIbanValidatedModel model = new()
        {
            SelectedAccountId = 3,
            RecipientName = "Jane Receiver",
            RecipientIban = "RO49AAAA1B31007593840000",
            RecipientBankName = "Validated Bank",
            Amount = 50,
            Currency = "RON",
            Reference = "Invoice 123"
        };

        _transferServiceMock
            .Setup(service => service.GetAccountsAsync(CancellationToken.None))
            .ReturnsAsync(Error.Failure("transfers.accounts_failed", "Accounts could not be loaded."));

        IActionResult result = await _controller.Preview(model, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("Preview");
        TransferPreviewModel previewModel = viewResult.Model.Should().BeOfType<TransferPreviewModel>().Subject;
        previewModel.SourceAccountId.Should().Be(3);
        previewModel.SourceAccountIban.Should().BeEmpty();
        previewModel.SourceCurrency.Should().BeEmpty();
        previewModel.IsCrossCurrency.Should().BeFalse();
        previewModel.ExchangeRate.Should().BeNull();
        previewModel.ConvertedAmount.Should().BeNull();
        _transferServiceMock.VerifyAll();
        _beneficiaryServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Preview_WhenForexPreviewFails_ShouldReturnValidatedIbanViewWithModelError()
    {
        TransferIbanValidatedModel model = new()
        {
            SelectedAccountId = 3,
            RecipientName = "Jane Receiver",
            RecipientIban = "RO49AAAA1B31007593840000",
            RecipientBankName = "Validated Bank",
            Amount = 150,
            Currency = "EUR",
            Reference = "Invoice 123"
        };

        List<TransferAccountSelectionResponse> transferAccounts =
        [
            new()
            {
                Id = 3,
                AccountName = "Daily Account",
                Iban = "RO11AAAA1B31007593840001",
                Currency = "RON",
                Balance = 1200.50m
            }
        ];

        _transferServiceMock
            .SetupSequence(service => service.GetAccountsAsync(CancellationToken.None))
            .ReturnsAsync(transferAccounts)
            .ReturnsAsync(transferAccounts);

        _transferServiceMock
            .Setup(service => service.GetFxPreviewAsync("RON", "EUR", 150, CancellationToken.None))
            .ReturnsAsync(Error.Failure("transfers.fx_failed", "FX preview failed."));

        IActionResult result = await _controller.Preview(model, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("ValidateIban");
        viewResult.Model.Should().Be(model);
        model.Accounts.Should().BeEquivalentTo(transferAccounts);
        _controller.ModelState[string.Empty]!.Errors
            .Should().ContainSingle(error => error.ErrorMessage == "Unable to fetch forex rate. Please try again.");
        _transferServiceMock.VerifyAll();
        _beneficiaryServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Preview_WhenTargetCurrencyIsMissing_ShouldReturnPreviewWithoutForexData()
    {
        TransferIbanValidatedModel model = new()
        {
            SelectedAccountId = 3,
            RecipientName = "Jane Receiver",
            RecipientIban = "RO49AAAA1B31007593840000",
            RecipientBankName = "Validated Bank",
            Amount = 150,
            Currency = string.Empty,
            Reference = "Invoice 123"
        };

        _transferServiceMock
            .Setup(service => service.GetAccountsAsync(CancellationToken.None))
            .ReturnsAsync(new List<TransferAccountSelectionResponse>
            {
                new()
                {
                    Id = 3,
                    AccountName = "Daily Account",
                    Iban = "RO11AAAA1B31007593840001",
                    Currency = "RON",
                    Balance = 1200.50m
                }
            });

        IActionResult result = await _controller.Preview(model, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("Preview");
        TransferPreviewModel previewModel = viewResult.Model.Should().BeOfType<TransferPreviewModel>().Subject;
        previewModel.SourceAccountIban.Should().Be("RO11AAAA1B31007593840001");
        previewModel.SourceCurrency.Should().Be("RON");
        previewModel.IsCrossCurrency.Should().BeFalse();
        previewModel.ExchangeRate.Should().BeNull();
        previewModel.ConvertedAmount.Should().BeNull();
        _transferServiceMock.VerifyAll();
        _beneficiaryServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Preview_WhenForexPreviewReturnsParityRate_ShouldReturnPreviewWithoutCrossCurrencyFlag()
    {
        TransferIbanValidatedModel model = new()
        {
            SelectedAccountId = 3,
            RecipientName = "Jane Receiver",
            RecipientIban = "RO49AAAA1B31007593840000",
            RecipientBankName = "Validated Bank",
            Amount = 150,
            Currency = "EUR",
            Reference = "Invoice 123"
        };

        _transferServiceMock
            .Setup(service => service.GetAccountsAsync(CancellationToken.None))
            .ReturnsAsync(new List<TransferAccountSelectionResponse>
            {
                new()
                {
                    Id = 3,
                    AccountName = "Daily Account",
                    Iban = "RO11AAAA1B31007593840001",
                    Currency = "RON",
                    Balance = 1200.50m
                }
            });

        _transferServiceMock
            .Setup(service => service.GetFxPreviewAsync("RON", "EUR", 150, CancellationToken.None))
            .ReturnsAsync(new TransferForexPreviewResponse
            {
                ExchangeRate = 1,
                ConvertedAmount = 150
            });

        IActionResult result = await _controller.Preview(model, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        TransferPreviewModel previewModel = viewResult.Model.Should().BeOfType<TransferPreviewModel>().Subject;
        previewModel.IsCrossCurrency.Should().BeFalse();
        previewModel.ExchangeRate.Should().Be(1);
        previewModel.ConvertedAmount.Should().Be(150);
        _transferServiceMock.VerifyAll();
        _beneficiaryServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Preview_WhenForexPreviewReturnsNonParityRate_ShouldReturnCrossCurrencyPreview()
    {
        TransferIbanValidatedModel model = new()
        {
            SelectedAccountId = 3,
            RecipientName = "Jane Receiver",
            RecipientIban = "RO49AAAA1B31007593840000",
            RecipientBankName = "Validated Bank",
            Amount = 150,
            Currency = "EUR",
            Reference = "Invoice 123"
        };

        _transferServiceMock
            .Setup(service => service.GetAccountsAsync(CancellationToken.None))
            .ReturnsAsync(new List<TransferAccountSelectionResponse>
            {
                new()
                {
                    Id = 3,
                    AccountName = "Daily Account",
                    Iban = "RO11AAAA1B31007593840001",
                    Currency = "RON",
                    Balance = 1200.50m
                }
            });

        _transferServiceMock
            .Setup(service => service.GetFxPreviewAsync("RON", "EUR", 150, CancellationToken.None))
            .ReturnsAsync(new TransferForexPreviewResponse
            {
                ExchangeRate = 4.95m,
                ConvertedAmount = 30.30m
            });

        IActionResult result = await _controller.Preview(model, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("Preview");
        TransferPreviewModel previewModel = viewResult.Model.Should().BeOfType<TransferPreviewModel>().Subject;
        previewModel.IsCrossCurrency.Should().BeTrue();
        previewModel.ExchangeRate.Should().Be(4.95m);
        previewModel.ConvertedAmount.Should().Be(30.30m);
        _transferServiceMock.VerifyAll();
        _beneficiaryServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Confirm_WhenTransferExecutionFails_ShouldRedirectToNewAndSetTempData()
    {
        TransferPreviewModel model = new()
        {
            SourceAccountId = 3,
            RecipientName = "Jane Receiver",
            RecipientIban = "RO49AAAA1B31007593840000",
            Amount = 150,
            Currency = "EUR",
            Reference = "Invoice 123"
        };

        _transferServiceMock
            .Setup(service => service.ExecuteAsync(
                It.Is<CreateTransferRequest>(request =>
                    request.SourceAccountId == 3 &&
                    request.RecipientName == "Jane Receiver" &&
                    request.RecipientIban == "RO49AAAA1B31007593840000" &&
                    request.Amount == 150 &&
                    request.Currency == "EUR" &&
                    request.Reference == "Invoice 123"),
                CancellationToken.None))
            .ReturnsAsync(Error.Failure("transfers.execute_failed", "Transfer could not be completed."));

        IActionResult result = await _controller.Confirm(model, CancellationToken.None);

        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(TransfersController.New));
        _controller.TempData["Error"].Should().Be("Transfer could not be completed.");
        _transferServiceMock.VerifyAll();
        _beneficiaryServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Confirm_WhenTransferExecutionSucceeds_ShouldRedirectToHistoryAndSetSuccessTempData()
    {
        TransferPreviewModel model = new()
        {
            SourceAccountId = 3,
            RecipientName = "Jane Receiver",
            RecipientIban = "RO49AAAA1B31007593840000",
            Amount = 150,
            Currency = "EUR",
            Reference = "Invoice 123"
        };

        _transferServiceMock
            .Setup(service => service.ExecuteAsync(It.IsAny<CreateTransferRequest>(), CancellationToken.None))
            .ReturnsAsync(new TransferExecutionResponse { TransactionRef = "TRX-123" });

        IActionResult result = await _controller.Confirm(model, CancellationToken.None);

        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(TransfersController.History));
        _controller.TempData["Success"].Should().Be("Transfer completed! Ref: TRX-123 — EUR 150.00 to Jane Receiver.");
        _transferServiceMock.VerifyAll();
        _beneficiaryServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task History_WhenTransferServiceReturnsError_ShouldReturnEmptyViewAndSetTempData()
    {
        _transferServiceMock
            .Setup(service => service.GetHistoryAsync(CancellationToken.None))
            .ReturnsAsync(Error.Failure("transfers.history_failed", "History could not be loaded."));

        IActionResult result = await _controller.History(CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        TransferHistoryModel model = viewResult.Model.Should().BeOfType<TransferHistoryModel>().Subject;
        model.HasTransfers.Should().BeFalse();
        _controller.TempData["Error"].Should().Be("Unable to load transfer history. Please try again.");
        _transferServiceMock.VerifyAll();
        _beneficiaryServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task History_WhenTransferServiceReturnsTransfers_ShouldMapTransferRows()
    {
        DateTime createdAt = new(2026, 6, 4, 12, 30, 0, DateTimeKind.Utc);

        _transferServiceMock
            .Setup(service => service.GetHistoryAsync(CancellationToken.None))
            .ReturnsAsync(new List<TransferResponse>
            {
                new()
                {
                    Id = 11,
                    RecipientIban = "RO49AAAA1B31007593840000",
                    RecipientName = "Jane Receiver",
                    Amount = 150,
                    Currency = "EUR",
                    Status = TransferStatus.Completed,
                    TransactionRef = "",
                    CreatedAt = createdAt
                }
            });

        IActionResult result = await _controller.History(CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        TransferHistoryModel model = viewResult.Model.Should().BeOfType<TransferHistoryModel>().Subject;
        model.HasTransfers.Should().BeTrue();
        model.Transfers.Should().ContainSingle();
        model.Transfers[0].Id.Should().Be(11);
        model.Transfers[0].RecipientIban.Should().Be("RO49AAAA1B31007593840000");
        model.Transfers[0].RecipientName.Should().Be("Jane Receiver");
        model.Transfers[0].Amount.Should().Be(150);
        model.Transfers[0].Currency.Should().Be("EUR");
        model.Transfers[0].Status.Should().Be(nameof(TransferStatus.Completed));
        model.Transfers[0].Reference.Should().Be("—");
        model.Transfers[0].CreatedAt.Should().Be(createdAt);
        _transferServiceMock.VerifyAll();
        _beneficiaryServiceMock.VerifyNoOtherCalls();
    }
}
