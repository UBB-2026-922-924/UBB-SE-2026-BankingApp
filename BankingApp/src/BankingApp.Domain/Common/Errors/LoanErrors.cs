namespace BankingApp.Domain.Common.Errors;

using ErrorOr;

public static class LoanErrors
{
    public static Error LoanNotFound => Error.NotFound("Loan.LoanNotFound", "The loan was not found.");
    public static Error ApplicationNotFound => Error.NotFound("Loan.ApplicationNotFound", "The loan application was not found.");
    public static Error ApplicationAlreadyProcessed => Error.Conflict("Loan.ApplicationAlreadyProcessed", "This loan application has already been approved or rejected.");
    public static Error InvalidPaymentAmount => Error.Validation("Loan.InvalidPaymentAmount", "Payment amount must be greater than zero.");
    public static Error LoanAlreadyClosed => Error.Conflict("Loan.LoanAlreadyClosed", "The loan has already been fully repaid.");
}
