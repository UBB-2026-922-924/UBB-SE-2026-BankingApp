namespace BankingApp.Application.Features.Billers.Services;

using Contracts.Features.Billers.Dtos;
using Domain.Aggregates.SavedBillerAggregate;
using Domain.Common.Errors;
using Domain.ReferenceData.Billers;
using Domain.Repositories;
using ErrorOr;
using Shared.Clock;
using Shared.Persistence;

public sealed class BillerService(
    IBillerRepository billerRepository,
    ISavedBillerRepository savedBillerRepository,
    IUnitOfWork unitOfWork,
    ISystemClock clock)
    : IBillerService
{
    public async Task<ErrorOr<List<BillerDto>>> GetBillersAsync(string? search = null, string? category = null, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Biller> billers = await billerRepository.ListActiveAsync(cancellationToken);

        IEnumerable<Biller> filtered = billers;

        if (!string.IsNullOrWhiteSpace(search))
        {
            filtered = filtered.Where(biller => biller.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            filtered = filtered.Where(biller => biller.Category.ToString().Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        return filtered
            .Select(biller => new BillerDto
            {
                Id = biller.Id,
                Name = biller.Name,
                Category = biller.Category.ToString(),
                LogoUrl = biller.LogoUrl,
                IsActive = biller.IsActive
            })
            .ToList();
    }

    public async Task<ErrorOr<List<SavedBillerDto>>> GetSavedBillersAsync(int userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<SavedBiller> savedBillers = await savedBillerRepository.ListByUserIdAsync(userId, cancellationToken);

        List<SavedBillerDto> result = [];
        foreach (SavedBiller savedBiller in savedBillers)
        {
            Biller? biller = await billerRepository.GetByIdAsync(savedBiller.BillerId, cancellationToken);
            result.Add(new SavedBillerDto
            {
                Id = savedBiller.Id,
                UserId = savedBiller.UserId,
                BillerId = savedBiller.BillerId,
                BillerName = biller?.Name ?? string.Empty,
                BillerCategory = biller?.Category.ToString() ?? string.Empty,
                LogoUrl = biller?.LogoUrl,
                Nickname = savedBiller.Nickname,
                DefaultReference = savedBiller.DefaultReference,
                CreatedAt = savedBiller.CreatedAt,
                Biller = biller is null ? null : new BillerDto
                {
                    Id = biller.Id,
                    Name = biller.Name,
                    Category = biller.Category.ToString(),
                    LogoUrl = biller.LogoUrl,
                    IsActive = biller.IsActive
                }
            });
        }

        return result;
    }

    public async Task<ErrorOr<SavedBillerDto>> SaveBillerAsync(int userId, int billerId, string? nickname, string? defaultReference, CancellationToken cancellationToken = default)
    {
        Biller? biller = await billerRepository.GetByIdAsync(billerId, cancellationToken);
        if (biller is null)
        {
            return BillerErrors.BillerNotFound;
        }

        IReadOnlyCollection<SavedBiller> existing = await savedBillerRepository.ListByUserIdAsync(userId, cancellationToken);
        if (existing.Any(s => s.BillerId == billerId))
        {
            return BillerErrors.BillerAlreadySaved;
        }

        var saved = SavedBiller.Create(userId, billerId, nickname, defaultReference, clock.UtcNow);

        await savedBillerRepository.AddAsync(saved, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SavedBillerDto
        {
            Id = saved.Id,
            UserId = saved.UserId,
            BillerId = saved.BillerId,
            BillerName = biller.Name,
            BillerCategory = biller.Category.ToString(),
            LogoUrl = biller.LogoUrl,
            Nickname = saved.Nickname,
            DefaultReference = saved.DefaultReference,
            CreatedAt = saved.CreatedAt,
            Biller = new BillerDto
            {
                Id = biller.Id,
                Name = biller.Name,
                Category = biller.Category.ToString(),
                LogoUrl = biller.LogoUrl,
                IsActive = biller.IsActive
            }
        };
    }

    public async Task<ErrorOr<Success>> DeleteSavedBillerAsync(int userId, int savedBillerId, CancellationToken cancellationToken = default)
    {
        SavedBiller? savedBiller = await savedBillerRepository.GetByIdAsync(savedBillerId, cancellationToken);
        if (savedBiller is null || savedBiller.UserId != userId)
        {
            return BillerErrors.SavedBillerNotFound;
        }

        await savedBillerRepository.DeleteAsync(savedBiller, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}
