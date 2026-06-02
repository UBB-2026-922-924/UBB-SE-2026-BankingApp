namespace BankingApp.Web.Tests.Controllers;

using Contracts.Features.Beneficiaries.Dtos;
using Contracts.Features.Beneficiaries.Services;
using BankingApp.Web.Controllers;
using BankingApp.Web.Models.Beneficiaries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public sealed class BeneficiariesControllerTests : IDisposable
{
    private readonly Mock<IBeneficiaryService> _beneficiaryService = new(MockBehavior.Strict);
    private readonly BeneficiariesController _controller;

    public BeneficiariesControllerTests()
    {
        DefaultHttpContext httpContext = new();
        _controller = new BeneficiariesController(_beneficiaryService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
        };
    }

    public void Dispose() => _controller.Dispose();

    [Fact]
    public async Task Index_WhenServiceReturnsBeneficiaries_ReturnsModelWithMappedRows()
    {
        _beneficiaryService
            .Setup(service => service.GetAllAsync(CancellationToken.None))
            .ReturnsAsync(new List<BeneficiaryDto>
            {
                new()
                {
                    Id = 2,
                    Name = "Charlie",
                    Iban = "RO49AAAA1B31007593840000",
                    BankName = "Test Bank",
                    TransferCount = 4,
                    TotalAmountSent = 123.45m
                }
            });

        IActionResult result = await _controller.Index(CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        BeneficiaryListModel model = viewResult.Model.Should().BeOfType<BeneficiaryListModel>().Subject;
        model.HasBeneficiaries.Should().BeTrue();
        model.Beneficiaries.Should().ContainSingle();
        model.Beneficiaries[0].Name.Should().Be("Charlie");
        model.Beneficiaries[0].TransferCount.Should().Be(4);
        _beneficiaryService.VerifyAll();
    }

    [Fact]
    public async Task Create_WhenServiceSucceeds_MapsFormModelAndRedirects()
    {
        BeneficiaryFormModel beneficiaryForm = new()
        {
            Name = "  Jane Doe  ",
            Iban = "ro49 aaaa1b31007593840000",
            BankName = "  ING  "
        };

        _beneficiaryService
            .Setup(service => service.CreateAsync(
                It.Is<CreateBeneficiaryRequest>(request =>
                    request.Name == "  Jane Doe  " &&
                    request.Iban == "ro49 aaaa1b31007593840000" &&
                    request.BankName == "  ING  "),
                CancellationToken.None))
            .ReturnsAsync(Result.Success);

        IActionResult result = await _controller.Create(beneficiaryForm, CancellationToken.None);

        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(BeneficiariesController.Index));
        _controller.TempData["Success"].Should().Be("Jane Doe was added to your beneficiaries.");
        _beneficiaryService.VerifyAll();
    }

    [Fact]
    public async Task Edit_WhenServiceReturnsBeneficiary_ReturnsFormModel()
    {
        _beneficiaryService
            .Setup(service => service.GetByIdAsync(9, CancellationToken.None))
            .ReturnsAsync(new BeneficiaryDto
            {
                Id = 9,
                Name = "Saved Beneficiary",
                Iban = "RO49AAAA1B31007593840000",
                BankName = "Sample Bank"
            });

        IActionResult result = await _controller.Edit(9, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        BeneficiaryFormModel model = viewResult.Model.Should().BeOfType<BeneficiaryFormModel>().Subject;
        model.Id.Should().Be(9);
        model.Name.Should().Be("Saved Beneficiary");
        _beneficiaryService.VerifyAll();
    }
}
