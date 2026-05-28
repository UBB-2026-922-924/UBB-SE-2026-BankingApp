namespace BankingApp.Api.Tests.Integration;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BankingApp.Api.Tests.Integration.Infrastructure;
using BankingApp.Application.Features.Authentication.Models;
using BankingApp.Contracts.Features.Authentication.Dtos;
using BankingApp.Contracts.Http;
using ErrorOr;

public class AuthEndpointsTests : IClassFixture<BankingAppWebFactory>
{
    private readonly HttpClient _client;
    private readonly BankingAppWebFactory _factory;
    private readonly CancellationToken _cancellationToken;

    public AuthEndpointsTests(BankingAppWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _cancellationToken = TestContext.Current.CancellationToken;

        _factory.AuthServiceMock.Reset();
        _factory.JwtServiceMock.Reset();
        _factory.IdentityRepositoryMock.Reset();
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ShouldReturnOkWithToken()
    {
        _factory.AuthServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SessionMetadata?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<LoginSuccess>)new LoginSuccess(1, "fake-jwt-token", 42));

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/" + ApiEndpoints.Auth.LoginFull,
            new { Email = "test@example.com", Password = "Password1!" },
            _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        LoginSuccessResponse? result = await response.Content.ReadFromJsonAsync<LoginSuccessResponse>(_cancellationToken);
        result.Should().NotBeNull();
        result!.UserId.Should().Be(1);
        result.Token.Should().Be("fake-jwt-token");
    }

    [Fact]
    public async Task Login_WhenCredentialsAreInvalid_ShouldReturnUnauthorized()
    {
        _factory.AuthServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SessionMetadata?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Unauthorized("invalid_credentials", "Invalid credentials."));

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/" + ApiEndpoints.Auth.LoginFull,
            new { Email = "test@example.com", Password = "WrongPassword!" },
            _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_WhenValid_ShouldReturnNoContent()
    {
        _factory.AuthServiceMock
            .Setup(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/" + ApiEndpoints.Auth.RegisterFull,
            new { Email = "new@example.com", Password = "Password1!", FullName = "Test User" },
            _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Register_WhenEmailAlreadyExists_ShouldReturnConflict()
    {
        _factory.AuthServiceMock
            .Setup(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Conflict("email_taken", "Email is already registered."));

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/" + ApiEndpoints.Auth.RegisterFull,
            new { Email = "existing@example.com", Password = "Password1!", FullName = "Test User" },
            _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Logout_WhenTokenIsProvided_ShouldReturnNoContent()
    {
        _factory.AuthServiceMock
            .Setup(s => s.LogoutAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var request = new HttpRequestMessage(HttpMethod.Post, "/" + ApiEndpoints.Auth.LogoutFull);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "valid-token");

        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_WhenTokenIsMissing_ShouldReturnBadRequest()
    {
        HttpResponseMessage response = await _client.SendAsync(
            new HttpRequestMessage(HttpMethod.Post, "/" + ApiEndpoints.Auth.LogoutFull),
            _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
