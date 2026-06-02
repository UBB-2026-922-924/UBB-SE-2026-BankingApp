namespace BankingApp.Domain.Enums;

/// <summary>Represents the lifecycle state of a loan.</summary>
public enum LoanStatus
{
    /// <summary>The loan is active and being repaid.</summary>
    Active,

    /// <summary>The loan has passed its planned lifecycle status.</summary>
    Passed,
}
