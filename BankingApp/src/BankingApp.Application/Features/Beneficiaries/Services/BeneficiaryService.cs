namespace BankingApp.Application.Features.Beneficiaries.Services;

using Contracts.Features.Beneficiaries.Dtos;
using Domain.Aggregates.BeneficiaryAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Shared.Clock;
using Shared.Persistence;
using ApplicationLogMessages = Common.Logging.ApplicationLogMessages;

public sealed class BeneficiaryService(
    IBeneficiaryRepository beneficiaryRepository,
    IUnitOfWork unitOfWork,
    ISystemClock clock,
    ILogger<BeneficiaryService> logger)
    : IBeneficiaryService
{
    public async Task<ErrorOr<List<BeneficiaryDto>>> GetAllAsync(int userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Beneficiary> beneficiaries = await beneficiaryRepository.ListByUserIdAsync(userId, cancellationToken);

        return beneficiaries
            .Select(beneficiary => new BeneficiaryDto
            {
                Id = beneficiary.Id,
                UserId = beneficiary.UserId,
                Name = beneficiary.Name,
                Iban = beneficiary.Iban.Value,
                BankName = beneficiary.BankName,
                LastTransferDate = beneficiary.LastTransferDate,
                TotalAmountSent = beneficiary.TotalAmountSent,
                TransferCount = beneficiary.TransferCount
            })
            .ToList();
    }

    public async Task<ErrorOr<BeneficiaryDto>> CreateAsync(int userId, string name, string iban, string? bankName, CancellationToken cancellationToken = default)
    {
        ErrorOr<Iban> ibanResult = Iban.Create(iban);
        if (ibanResult.IsError)
        {
            return ibanResult.FirstError;
        }

        IReadOnlyCollection<Beneficiary> existing = await beneficiaryRepository.ListByUserIdAsync(userId, cancellationToken);
        if (existing.Any(b => b.Iban.Value.Equals(iban, StringComparison.OrdinalIgnoreCase)))
        {
            return BeneficiaryErrors.Duplicate;
        }

        var beneficiary = Beneficiary.Create(userId, name.Trim(), ibanResult.Value, bankName, clock.UtcNow);
        await beneficiaryRepository.AddAsync(beneficiary, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ApplicationLogMessages.BeneficiaryCreated(logger, beneficiary.Id, userId);

        return new BeneficiaryDto
        {
            Id = beneficiary.Id,
            UserId = beneficiary.UserId,
            Name = beneficiary.Name,
            Iban = beneficiary.Iban.Value,
            BankName = beneficiary.BankName,
            LastTransferDate = beneficiary.LastTransferDate,
            TotalAmountSent = beneficiary.TotalAmountSent,
            TransferCount = beneficiary.TransferCount
        };
    }

    public async Task<ErrorOr<Success>> UpdateAsync(int userId, int beneficiaryId, string name, string iban, string? bankName, CancellationToken cancellationToken = default)
    {
        Beneficiary? beneficiary = await beneficiaryRepository.GetByIdAsync(beneficiaryId, cancellationToken);
        if (beneficiary is null || beneficiary.UserId != userId)
        {
            return BeneficiaryErrors.NotFound;
        }

        ErrorOr<Iban> ibanResult = Iban.Create(iban);
        if (ibanResult.IsError)
        {
            return ibanResult.FirstError;
        }

        beneficiary.Update(name.Trim(), ibanResult.Value, bankName);
        await beneficiaryRepository.UpdateAsync(beneficiary, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> DeleteAsync(int userId, int beneficiaryId, CancellationToken cancellationToken = default)
    {
        Beneficiary? beneficiary = await beneficiaryRepository.GetByIdAsync(beneficiaryId, cancellationToken);
        if (beneficiary is null || beneficiary.UserId != userId)
        {
            return BeneficiaryErrors.NotFound;
        }

        await beneficiaryRepository.DeleteAsync(beneficiary, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}
