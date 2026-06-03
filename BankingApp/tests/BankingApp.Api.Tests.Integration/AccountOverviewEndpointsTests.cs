namespace BankingApp.Api.Tests.Integration;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BankingApp.Api.Tests.Integration.Infrastructure;
using BankingApp.Contracts.Features.AccountOverview.Dtos;
using BankingApp.Contracts.Http;
using BankingApp.Domain.Aggregates.IdentityAggregate;

public class AccountOverviewEndpointsTests : IClassFixture<BankingAppWebFactory>
{
    private const string ValidToken = "valid-token";
    private const int ValidUserId = 1;
    private const string DashboardRoute = "/" + ApiEndpoints.AccountOverview.Base;

    private readonly HttpClient _client;
    private readonly BankingAppWebFactory _factory;
    private readonly CancellationToken _cancellationToken;

    public AccountOverviewEndpointsTests(BankingAppWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _cancellationToken = TestContext.Current.CancellationToken;

        _factory.AccountOverviewServiceMock.Reset();
        _factory.JwtServiceMock.Reset();
        _factory.IdentityRepositoryMock.Reset();

        _factory.JwtServiceMock
            .Setup(jwtService => jwtService.ExtractUserId(ValidToken))
            .Returns(ValidUserId);

        _factory.IdentityRepositoryMock
            .Setup(repository => repository.GetBySessionTokenAsync(ValidToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateActiveIdentity(ValidUserId, ValidToken));
    }

    [Fact]
    public async Task GetDashboard_WhenUserIsAuthenticated_ShouldReturnOkWithAccountOverview()
    {
        AccountOverviewDto expectedDashboard = BuildSampleDashboard();

        _factory.AccountOverviewServiceMock
            .Setup(service => service.GetDashboardAsync(ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDashboard);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, DashboardRoute);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AccountOverviewDto? result = await response.Content.ReadFromJsonAsync<AccountOverviewDto>(_cancellationToken);

        result.Should().NotBeNull();
        result!.CurrentUser.Should().NotBeNull();
        result.CurrentUser!.FullName.Should().Be("Jane Doe");
        result.CurrentUser.Email.Should().Be("jane.doe@example.com");
        result.Cards.Should().HaveCount(1);
        result.Cards[0].AccountBalance.Should().Be(2500.75m);
        result.RecentTransactions.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetDashboard_WhenUserIsNotAuthenticated_ShouldReturnUnauthorized()
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, DashboardRoute);

        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static AccountOverviewDto BuildSampleDashboard()
    {
        return new AccountOverviewDto
        {
            CurrentUser = new UserSummaryDto
            {
                FullName = "Jane Doe",
                Email = "jane.doe@example.com",
                PhoneNumber = "0740000000"
            },
            Cards =
            [
                new CardDto
                {
                    CardNumber = "**** **** **** 1234",
                    AccountName = "Current Account",
                    AccountBalance = 2500.75m,
                    CardholderName = "Jane Doe",
                    ExpiryDate = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    IsContactlessEnabled = true,
                    IsOnlineEnabled = true
                }
            ],
            RecentTransactions =
            [
                new TransactionDto
                {
                    Description = "Coffee Shop",
                    Amount = -4.50m
                }
            ],
            UnreadNotificationCount = 3,
            PendingTransfers = []
        };
    }

    private static IdentityAccount CreateActiveIdentity(int userId, string token)
    {
        IdentityAccount identity = IdentityAccount.Create(userId, null);
        identity.OpenSession(token, DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow);
        return identity;
    }
}
