namespace BankingApp.Domain.Aggregates.LoanAggregate;

public class LoanEstimate(decimal indicativeRate, decimal monthlyInstallment, decimal totalRepayable)
    : IEquatable<LoanEstimate>
{
    public int Id { get; set; }

    public decimal IndicativeRate { get; } = indicativeRate;

    /// <summary>
    /// Gets or sets the projected monthly installment.
    /// </summary>
    public decimal MonthlyInstallment { get; } = monthlyInstallment;

    /// <summary>
    /// Gets or sets the estimated total amount repayable over the term.
    /// </summary>
    public decimal TotalRepayable { get; } = totalRepayable;

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return Equals(obj as LoanEstimate);
    }

    /// <summary>
    /// Determines whether the specified LoanEstimate is equal to the current LoanEstimate.
    /// </summary>
    public bool Equals(LoanEstimate? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return IndicativeRate == other.IndicativeRate &&
               MonthlyInstallment == other.MonthlyInstallment &&
               TotalRepayable == other.TotalRepayable;
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(IndicativeRate, MonthlyInstallment, TotalRepayable);
    }

    public static bool operator ==(LoanEstimate left, LoanEstimate right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(LoanEstimate left, LoanEstimate right)
    {
        return !(left == right);
    }
}