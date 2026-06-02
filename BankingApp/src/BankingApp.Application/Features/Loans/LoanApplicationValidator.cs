namespace BankingApp.Application.Features.Loans;

using BankingApp.Contracts.Features.Loans.Dtos;
using BankingApp.Domain.Enums;
using ErrorOr;

/// <summary>Validates incoming loan application requests.</summary>
public sealed class LoanApplicationValidator
{
    private const decimal MinimumDesiredAmountExclusive = 0m;
    private const int MinimumTermMonthsExclusive = 0;

    public ErrorOr<Success> Validate(LoanApplicationRequest request)
    {
        if (request.DesiredAmount <= MinimumDesiredAmountExclusive)
        {
            return Error.Validation("LoanApplication.InvalidAmount", "Desired amount must be greater than zero.");
        }

        if (!Enum.IsDefined(typeof(LoanType), request.LoanType))
        {
            return Error.Validation("LoanApplication.InvalidLoanType", "Invalid loan type.");
        }

        if (request.PreferredTermMonths <= MinimumTermMonthsExclusive)
        {
            return Error.Validation("LoanApplication.InvalidTerm", "Term must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(request.Purpose))
        {
            return Error.Validation("LoanApplication.PurposeRequired", "Purpose is required.");
        }

        return Result.Success;
    }
}
