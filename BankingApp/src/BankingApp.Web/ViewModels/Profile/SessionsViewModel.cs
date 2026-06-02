namespace BankingApp.Web.ViewModels.Profile;

using Contracts.Features.UserProfile.Dtos;

public sealed class SessionsViewModel
{
    public IList<SessionDto> Sessions { get; init; } = [];

    public int CurrentSessionId { get; init; }
}
