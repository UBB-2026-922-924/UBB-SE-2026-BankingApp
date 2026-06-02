namespace BankingApp.Domain.Aggregates.LoanAggregate.Entities;

using BankingApp.Domain.Common.Errors;
using BankingApp.Domain.Common.Primitives;
using BankingApp.Domain.Enums;
using ErrorOr;

public sealed class LoanApplication : Entity<int>
{
    private LoanApplication()
    {
    }

    private LoanApplication(int userId, LoanType loanType, decimal desiredAmount, int preferredTermMonths, string purpose)
    {
        UserId = userId;
        LoanType = loanType;
        DesiredAmount = desiredAmount;
        PreferredTermMonths = preferredTermMonths;
        Purpose = purpose;
        ApplicationStatus = LoanApplicationStatus.Pending;
    }

    public int UserId { get; private set; }
    public LoanType LoanType { get; private set; }
    public decimal DesiredAmount { get; private set; }
    public int PreferredTermMonths { get; private set; }
    public string Purpose { get; private set; } = string.Empty;
    public LoanApplicationStatus ApplicationStatus { get; private set; }
    public string? RejectionReason { get; private set; }

    public static LoanApplication Create(int userId, LoanType loanType, decimal desiredAmount, int preferredTermMonths, string purpose)
        => new(userId, loanType, desiredAmount, preferredTermMonths, purpose);

    /// <summary>Marks this application as approved.</summary>
    public ErrorOr<Success> Approve()
    {
        if (ApplicationStatus != LoanApplicationStatus.Pending)
        {
            return LoanErrors.ApplicationAlreadyProcessed;
        }

        ApplicationStatus = LoanApplicationStatus.Approved;
        return Result.Success;
    }

    /// <summary>Marks this application as rejected with the supplied reason.</summary>
    public ErrorOr<Success> Reject(string reason)
    {
        if (ApplicationStatus != LoanApplicationStatus.Pending)
        {
            return LoanErrors.ApplicationAlreadyProcessed;
        }

        ApplicationStatus = LoanApplicationStatus.Rejected;
        RejectionReason = reason;
        return Result.Success;
    }
}
