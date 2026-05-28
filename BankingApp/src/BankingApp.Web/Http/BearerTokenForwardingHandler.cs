namespace BankingApp.Web.Http;

using System.Net.Http.Headers;
using BankingApp.Contracts.Http;

public sealed class BearerTokenForwardingHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string? token = httpContextAccessor.HttpContext?.User.FindFirst(AuthClaimTypes.Token)?.Value;

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(AuthHeaderNames.BearerScheme, token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
