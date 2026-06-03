namespace BankingApp.Api.Tests.Integration;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BankingApp.Api.Tests.Integration.Infrastructure;
using BankingApp.Application.Features.Transfers.Services;
using BankingApp.Contracts.Features.Transfers.Dtos;
using BankingApp.Contracts.Http;
using BankingApp.Domain.Aggregates.IdentityAggregate;

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
        IdentityAccount identity = IdentityAccount.Create(userId, null);
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
}
