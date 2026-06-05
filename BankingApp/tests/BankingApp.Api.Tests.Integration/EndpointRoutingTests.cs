namespace BankingApp.Api.Tests.Integration;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BankingApp.Api.Tests.Integration.Infrastructure;
using BankingApp.Application.Features.Cards.Services;
using BankingApp.Contracts.Features.Cards.Dtos;
using BankingApp.Contracts.Http;
using BankingApp.Domain.Aggregates.IdentityAggregate;
using ErrorOr;

public class EndpointRoutingTests : IClassFixture<BankingAppWebFactory>
{
    private const string ValidToken = "valid-test-token";
    private const int TestUserId = 1;

    private readonly HttpClient _client;
    private readonly BankingAppWebFactory _factory;

    public EndpointRoutingTests(BankingAppWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        _factory.JwtServiceMock.Reset();
        _factory.IdentityRepositoryMock.Reset();
        _factory.CardServiceMock.Reset();

        factory.JwtServiceMock
            .Setup(service => service.ExtractUserId(ValidToken))
            .Returns(TestUserId);

        factory.IdentityRepositoryMock
            .Setup(repository => repository.GetBySessionTokenAsync(ValidToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateActiveIdentity(TestUserId, ValidToken));

        factory.CardServiceMock
            .Setup(service => service.GetCardsAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CardDetailsDto>());
    }

    [Theory]
    [InlineData("POST", "/" + ApiEndpoints.Auth.LoginFull)]
    [InlineData("POST", "/" + ApiEndpoints.Auth.RegisterFull)]
    [InlineData("POST", "/" + ApiEndpoints.Auth.LogoutFull)]
    public async Task SendAsync_WhenAuthEndpointIsPublicAndTokenIsMissing_ShouldNotReturnUnauthorized(
        string method,
        string path)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), path)
        {
            Content = JsonContent.Create(new { })
        };

        HttpResponseMessage response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("GET", "/" + ApiEndpoints.AccountOverview.Base)]
    [InlineData("GET", "/" + ApiEndpoints.Profile.Base)]
    [InlineData("PUT", "/" + ApiEndpoints.Profile.Base)]
    [InlineData("PUT", "/" + ApiEndpoints.Profile.ChangePasswordFull)]
    [InlineData("GET", "/" + ApiEndpoints.Profile.NotificationPreferencesFull)]
    [InlineData("PUT", "/" + ApiEndpoints.Profile.NotificationPreferencesFull)]
    [InlineData("POST", "/" + ApiEndpoints.Profile.VerifyPasswordFull)]
    [InlineData("GET", "/" + ApiEndpoints.Profile.SessionsFull)]
    [InlineData("DELETE", "/" + ApiEndpoints.Profile.SessionsFull + "/1")]
    [InlineData("GET", "/" + ApiEndpoints.Transfers.AccountsFull)]
    [InlineData("POST", "/" + ApiEndpoints.Transfers.ValidateIbanFull)]
    [InlineData("POST", "/" + ApiEndpoints.Transfers.ExecuteFull)]
    [InlineData("GET", "/" + ApiEndpoints.BillPayments.AccountsFull)]
    [InlineData("POST", "/" + ApiEndpoints.BillPayments.PayFull)]
    [InlineData("GET", "/" + ApiEndpoints.Forex.PreviewFull + "?from=RON&to=EUR&amount=100")]
    [InlineData("POST", "/" + ApiEndpoints.Forex.ExecuteFull)]
    [InlineData("GET", "/" + ApiEndpoints.Cards.Base)]
    public async Task SendAsync_WhenProtectedEndpointIsRequestedAndTokenIsMissing_ShouldReturnUnauthorized(
        string method,
        string path)
    {
        HttpResponseMessage response = await _client.SendAsync(
            new HttpRequestMessage(new HttpMethod(method), path),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("GET", "/" + ApiEndpoints.AccountOverview.Base)]
    [InlineData("GET", "/" + ApiEndpoints.Profile.Base)]
    [InlineData("PUT", "/" + ApiEndpoints.Profile.Base)]
    [InlineData("PUT", "/" + ApiEndpoints.Profile.ChangePasswordFull)]
    [InlineData("GET", "/" + ApiEndpoints.Profile.NotificationPreferencesFull)]
    [InlineData("PUT", "/" + ApiEndpoints.Profile.NotificationPreferencesFull)]
    [InlineData("POST", "/" + ApiEndpoints.Profile.VerifyPasswordFull)]
    [InlineData("GET", "/" + ApiEndpoints.Profile.SessionsFull)]
    [InlineData("DELETE", "/" + ApiEndpoints.Profile.SessionsFull + "/1")]
    [InlineData("GET", "/" + ApiEndpoints.Transfers.AccountsFull)]
    [InlineData("POST", "/" + ApiEndpoints.Transfers.ValidateIbanFull)]
    [InlineData("POST", "/" + ApiEndpoints.Transfers.ExecuteFull)]
    [InlineData("GET", "/" + ApiEndpoints.BillPayments.AccountsFull)]
    [InlineData("POST", "/" + ApiEndpoints.BillPayments.PayFull)]
    [InlineData("GET", "/" + ApiEndpoints.Forex.PreviewFull + "?from=RON&to=EUR&amount=100")]
    [InlineData("POST", "/" + ApiEndpoints.Forex.ExecuteFull)]
    [InlineData("GET", "/" + ApiEndpoints.Cards.Base)]
    public async Task SendAsync_WhenProtectedEndpointIsRequestedAndTokenIsValid_ShouldNotReturnUnauthorized(
        string method,
        string path)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        if (method is "POST" or "PUT")
        {
            request.Content = JsonContent.Create(new { });
        }

        HttpResponseMessage response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SendAsync_WhenProtectedEndpointIsRequestedAndTokenIsInvalid_ShouldReturnUnauthorized()
    {
        const string invalidToken = "bad-token";

        _factory.JwtServiceMock
            .Setup(service => service.ExtractUserId(invalidToken))
            .Returns(Error.Unauthorized("Token.Invalid", "Token is invalid."));

        var request = new HttpRequestMessage(HttpMethod.Get, "/" + ApiEndpoints.AccountOverview.Base);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", invalidToken);

        HttpResponseMessage response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendAsync_WhenProtectedEndpointIsRequestedAndSessionIsExpired_ShouldReturnUnauthorized()
    {
        const string orphanToken = "orphan-token";

        _factory.JwtServiceMock
            .Setup(service => service.ExtractUserId(orphanToken))
            .Returns(TestUserId);

        _factory.IdentityRepositoryMock
            .Setup(repository => repository.GetBySessionTokenAsync(orphanToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityAccount?)null);

        var request = new HttpRequestMessage(HttpMethod.Get, "/" + ApiEndpoints.AccountOverview.Base);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", orphanToken);

        HttpResponseMessage response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAsync_WhenRouteDoesNotExistAndTokenIsMissing_ShouldReturnUnauthorized()
    {
        HttpResponseMessage response = await _client.GetAsync(
            "/api/does-not-exist",
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendAsync_WhenRouteDoesNotExistAndTokenIsValid_ShouldReturnNotFound()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/does-not-exist");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        HttpResponseMessage response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static IdentityAccount CreateActiveIdentity(int userId, string token)
    {
        var identity = IdentityAccount.Create(userId, null);
        identity.OpenSession(token, DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow);
        return identity;
    }
}
