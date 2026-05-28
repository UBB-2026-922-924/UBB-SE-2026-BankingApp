namespace BankingApp.Application.Features.Cards.Services;

using Contracts.Features.Cards.Dtos;
using Domain.Enums;
using ErrorOr;

public interface ICardService
{
    public Task<ErrorOr<List<CardDetailsDto>>> GetCardsAsync(int userId, CancellationToken cancellationToken = default);
    public Task<ErrorOr<CardDetailsDto>> IssueAsync(int userId, CardType cardType, string? cardBrand, CancellationToken cancellationToken = default);
    public Task<ErrorOr<Success>> FreezeAsync(int userId, int cardId, CancellationToken cancellationToken = default);
    public Task<ErrorOr<Success>> UnfreezeAsync(int userId, int cardId, CancellationToken cancellationToken = default);
    public Task<ErrorOr<Success>> CancelAsync(int userId, int cardId, CancellationToken cancellationToken = default);
}
