namespace BankingApp.Contracts.Features.Cards.Services;

using Dtos;
using ErrorOr;

public interface ICardService
{
    public Task<ErrorOr<List<CardDetailsDto>>> GetCardsAsync(CancellationToken ct = default);
    public Task<ErrorOr<CardDetailsDto>> IssueCardAsync(IssueCardRequest request, CancellationToken ct = default);
    public Task<ErrorOr<Success>> FreezeCardAsync(int id, CancellationToken ct = default);
    public Task<ErrorOr<Success>> UnfreezeCardAsync(int id, CancellationToken ct = default);
    public Task<ErrorOr<Success>> CancelCardAsync(int id, CancellationToken ct = default);
}
