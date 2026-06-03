namespace BankingApp.Api.Tests.Integration;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BankingApp.Api.Tests.Integration.Infrastructure;
using BankingApp.Contracts.Features.Transfers.Dtos;
using BankingApp.Contracts.Http;
using BankingApp.Domain.Aggregates.IdentityAggregate;
using BankingApp.Domain.Enums;

public class TransferEndpointsTests : IClassFixture<BankingAppWebFactory>
{
    private const string ValidToken = "valid-token";
    private const int ValidUserId = 1;
    private const int ValidSourceAccountId = 42;
    private const string ValidRecipientName = "Ion Popescu";
    private const string ValidRecipientIban = "RO49AAAA1B31007593840000";
    private const string InvalidRecipientIban = "INVALID-IBAN";
    private const decimal ValidAmount = 500m;
    private const decimal ExcessiveAmount = 999999999m;
    private const string ValidCurrency = "RON";
    private const string ExecuteTransferRoute = "/" + ApiEndpoints.Transfers.Base + "/" + ApiEndpoints.Transfers.Execute;

    private readonly HttpClient _client;
    private readonly BankingAppWebFactory _factory;
    private readonly CancellationToken _cancellationToken;

    public TransferEndpointsTests(BankingAppWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _cancellationToken = TestContext.Current.CancellationToken;

        _factory.TransferServiceMock.Reset();
        _factory.JwtServiceMock.Reset();
        _factory.IdentityRepositoryMock.Reset();

        _factory.JwtServiceMock
            .Setup(jwtService => jwtService.ExtractUserId(ValidToken))
            .Returns(ValidUserId);

        _factory.IdentityRepositoryMock
            .Setup(repository => repository.GetBySessionTokenAsync(ValidToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateActiveIdentity(ValidUserId, ValidToken));
    }

    private static IdentityAccount CreateActiveIdentity(int userId, string token)
    {
        var identity = IdentityAccount.Create(userId, null);
        identity.OpenSession(token, DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow);
        return identity;
    }

    private static CreateTransferRequest BuildValidRequest()
    {
        return new CreateTransferRequest
        {
            SourceAccountId = ValidSourceAccountId,
            RecipientName = ValidRecipientName,
            RecipientIban = ValidRecipientIban,
            Amount = ValidAmount,
            Currency = ValidCurrency,
            Reference = "Test transfer"
        };
    }

    [Fact]
    public async Task ExecuteTransfer_WhenRequestIsValid_ShouldReturnOk()
    {
        var serviceResult = new TransferResponse
        {
            Id = 1,
            SourceAccountId = ValidSourceAccountId,
            RecipientName = ValidRecipientName,
            RecipientIban = ValidRecipientIban,
            Amount = ValidAmount,
            Currency = ValidCurrency,
            TransactionRef = "TRF-20260603-ABCDEF",
            Status = TransferStatus.Completed
        };

        _factory.TransferServiceMock
            .Setup(service => service.ExecuteAsync(
                ValidUserId,
                ValidSourceAccountId,
                ValidRecipientName,
                ValidRecipientIban,
                ValidAmount,
                ValidCurrency,
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var request = new HttpRequestMessage(HttpMethod.Post, ExecuteTransferRoute);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);
        request.Content = JsonContent.Create(BuildValidRequest());

        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        TransferExecutionResponse? result = await response.Content.ReadFromJsonAsync<TransferExecutionResponse>(_cancellationToken);

        result.Should().NotBeNull();
        result!.TransactionRef.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExecuteTransfer_WhenIbanIsInvalid_ShouldReturnBadRequest()
    {
        _factory.TransferServiceMock
            .Setup(service => service.ExecuteAsync(
                ValidUserId,
                ValidSourceAccountId,
                ValidRecipientName,
                InvalidRecipientIban,
                ValidAmount,
                ValidCurrency,
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Validation("Transfer.InvalidIban", "The recipient IBAN is not valid."));

        var requestBody = new CreateTransferRequest
        {
            SourceAccountId = ValidSourceAccountId,
            RecipientName = ValidRecipientName,
            RecipientIban = InvalidRecipientIban,
            Amount = ValidAmount,
            Currency = ValidCurrency,
            Reference = "Test transfer"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, ExecuteTransferRoute);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);
        request.Content = JsonContent.Create(requestBody);

        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
