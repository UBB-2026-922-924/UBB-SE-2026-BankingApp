namespace BankingApp.Domain.Enums;

/// <summary>Represents the review status of a loan application.</summary>
public enum LoanApplicationStatus
{
    /// <summary>The application is awaiting review.</summary>
    Pending,

    /// <summary>The application has been approved.</summary>
    Approved,

    /// <summary>The application has been rejected.</summary>
    Rejected,
}
