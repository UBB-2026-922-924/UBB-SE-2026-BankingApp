namespace BankingApp.Desktop.Tests.ViewModels;

using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Desktop.ViewModels;
using Contracts.Features.Beneficiaries.Dtos;
using Contracts.Features.Beneficiaries.Services;
using State;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Navigation;

public class BeneficiariesViewModelTests
{
    private readonly Mock<IBeneficiaryService> _beneficiaryClientService;
    private readonly BeneficiariesViewModel _viewModel;

    public BeneficiariesViewModelTests()
    {
        _beneficiaryClientService = new Mock<IBeneficiaryService>(MockBehavior.Strict);
        _viewModel = new BeneficiariesViewModel(
            _beneficiaryClientService.Object,
            Mock.Of<IAppNavigationService>(),
            Mock.Of<ITransferDraftState>(),
            NullLogger<BeneficiariesViewModel>.Instance);
    }

    [Fact]
    public async Task LoadBeneficiaries_WhenResponseIsValid_ShouldPopulateViewModel()
    {
        var data = new List<BeneficiaryDto>
        {
            new BeneficiaryDto { Id = 1, Name = "Alice", Iban = "DE123", BankName = "Bank A" },
            new BeneficiaryDto { Id = 2, Name = "Bob", Iban = "DE456", BankName = "Bank B" }
        };

        _beneficiaryClientService
            .Setup(service => service.GetAllAsync(default))
            .ReturnsAsync(data);

        ErrorOr<Success> result = await _viewModel.LoadBeneficiariesAsync();

        result.IsError.Should().BeFalse();
        _viewModel.Beneficiaries.Should().HaveCount(2);
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadBeneficiaries_WhenUnauthorized_ShouldSetErrorMessage()
    {
        _beneficiaryClientService
            .Setup(service => service.GetAllAsync(default))
            .ReturnsAsync(Error.Unauthorized());

        ErrorOr<Success> result = await _viewModel.LoadBeneficiariesAsync();

        result.IsError.Should().BeTrue();
        _viewModel.Beneficiaries.Should().BeEmpty();
        _viewModel.ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }
}
