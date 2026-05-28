namespace BankingApp.Api.Tests.Integration;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BankingApp.Api.Tests.Integration.Infrastructure;
using BankingApp.Contracts.Features.UserProfile.Dtos;
using BankingApp.Contracts.Http;
using BankingApp.Domain.Aggregates.IdentityAggregate;

public class ProfileEndpointsTests : IClassFixture<BankingAppWebFactory>
{
    private const string ValidToken = "valid-token";
    private const int ValidUserId = 1;

    private readonly HttpClient _client;
    private readonly BankingAppWebFactory _factory;
    private readonly CancellationToken _cancellationToken;

    public ProfileEndpointsTests(BankingAppWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _cancellationToken = TestContext.Current.CancellationToken;

        _factory.UserProfileServiceMock.Reset();
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
    public async Task GetProfile_WhenUserExists_ShouldReturnOkWithProfileInfo()
    {
        _factory.UserProfileServiceMock
            .Setup(s => s.GetProfileAsync(ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProfileDto
            {
                UserId = ValidUserId,
                Email = "user@test.com",
                FullName = "Test User"
            });

        var request = new HttpRequestMessage(HttpMethod.Get, "/" + ApiEndpoints.Profile.Base);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ProfileDto? result = await response.Content.ReadFromJsonAsync<ProfileDto>(_cancellationToken);
        result.Should().NotBeNull();
        result!.Email.Should().Be("user@test.com");
    }

    [Fact]
    public async Task UpdateProfile_WhenDataIsValid_ShouldReturnNoContent()
    {
        _factory.UserProfileServiceMock
            .Setup(s => s.UpdateProfileAsync(ValidUserId, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<DateTime?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var request = new HttpRequestMessage(HttpMethod.Put, "/" + ApiEndpoints.Profile.Base);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);
        request.Content = JsonContent.Create(new UpdateProfileRequest
        {
            PhoneNumber = "1234567890",
            Address = "123 Test St"
        });

        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ChangePassword_WhenOldPasswordIsIncorrect_ShouldReturnBadRequest()
    {
        _factory.UserProfileServiceMock
            .Setup(s => s.ChangePasswordAsync(ValidUserId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Validation("Password.Mismatch", "Old password does not match."));

        var request = new HttpRequestMessage(HttpMethod.Put, "/" + ApiEndpoints.Profile.ChangePasswordFull);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);
        request.Content = JsonContent.Create(new ChangePasswordRequest
        {
            CurrentPassword = "wrong",
            NewPassword = "newPassword1!"
        });

        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static IdentityAccount CreateActiveIdentity(int userId, string token)
    {
        var identity = IdentityAccount.Create(userId, null);
        identity.OpenSession(token, DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow);
        return identity;
    }
}
