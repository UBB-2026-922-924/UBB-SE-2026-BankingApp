namespace BankingApp.Web.Tests;

using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

public sealed class AuthorizationRoutingTests
{
    [Fact]
    public async Task Dashboard_WhenAnonymous_ShouldRedirectToLoginWithReturnUrl()
    {
        // Arrange
        await using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureAppConfiguration(configurationBuilder =>
                {
                    configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ApiBaseUrl"] = "http://localhost"
                    });
                });
            });

        using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        using HttpResponseMessage response = await client.GetAsync(
            "/Dashboard",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.PathAndQuery.Should().Be("/Auth/Login?returnUrl=%2FDashboard");
    }
}
