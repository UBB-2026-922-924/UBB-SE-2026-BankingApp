using System.Globalization;
using System.Security.Claims;
using BankingApp.Contracts.Features.Authentication.Dtos;
using BankingApp.Contracts.Http;
using BankingApp.Web.DependencyInjection;
using ErrorOr;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using ClientAuthenticationService = BankingApp.Application.Features.Authentication.Services.IAuthenticationService;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("ApiBaseUrl is not configured.");

int defaultCookieExpiryTime = 8;
int defaultLoginExpiryTime = 12;

builder.Services.AddControllersWithViews(options =>
{
    AuthorizationPolicy policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(policy));
});

builder.Services.AddWebClientServices(apiBaseUrl);
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.ReturnUrlParameter = "returnUrl";
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(defaultCookieExpiryTime);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/500");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapGet(
            "/dev/login",
            async (
                HttpContext context,
                ClientAuthenticationService authenticationService,
                IConfiguration configuration,
                string? returnUrl) =>
            {
                string? email = configuration["DevLogin:Email"];
                string? password = configuration["DevLogin:Password"];

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    return Results.Problem(
                        "Dev login is not configured. Set DevLogin:Email and DevLogin:Password in user secrets.",
                        statusCode: StatusCodes.Status500InternalServerError);
                }

                ErrorOr<LoginSuccessResponse> result = await authenticationService.LoginAsync(
                    new LoginRequest { Email = email, Password = password },
                    context.RequestAborted);

                if (result.IsError)
                {
                    return Results.Problem(
                        $"Dev login failed: {result.FirstError.Description}",
                        statusCode: StatusCodes.Status502BadGateway);
                }

                LoginSuccessResponse login = result.Value;

                if (string.IsNullOrWhiteSpace(login.Token))
                {
                    return Results.Problem(
                        "Dev login failed because the API did not return an authentication token.",
                        statusCode: StatusCodes.Status502BadGateway);
                }

                if (login.SessionId is null)
                {
                    return Results.Problem(
                        "Dev login failed because the API did not return a session identifier.",
                        statusCode: StatusCodes.Status502BadGateway);
                }

                string userId = login.UserId.ToString(CultureInfo.InvariantCulture);
                Claim[] claims =
                [
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Name, email),
                    new Claim(AuthClaimTypes.UserId, userId),
                    new Claim(AuthClaimTypes.Token, login.Token),
                    new Claim(AuthClaimTypes.SessionId, login.SessionId.Value.ToString(CultureInfo.InvariantCulture))
                ];

                ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                AuthenticationProperties properties = new()
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(defaultLoginExpiryTime)
                };

                await context.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    properties);

                return Results.Redirect(IsLocalReturnUrl(returnUrl) ? returnUrl! : "/");
            })
        .AllowAnonymous();

    app.MapPost(
            "/dev/logout",
            async (HttpContext context) =>
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return Results.Redirect("/");
            })
        .AllowAnonymous();
}

app.MapStaticAssets().AllowAnonymous();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
return;

static bool IsLocalReturnUrl(string? returnUrl)
{
    int firstLetterOfUrl = 0;
    int secondLetterOfUrl = 1;
    int rootPathLength = 1;
    return !string.IsNullOrEmpty(returnUrl)
           && returnUrl[firstLetterOfUrl] == '/'
           && (returnUrl.Length == rootPathLength || 
               (returnUrl[secondLetterOfUrl] != '/'
                && returnUrl[secondLetterOfUrl] != '\\'));
}

public partial class Program;
