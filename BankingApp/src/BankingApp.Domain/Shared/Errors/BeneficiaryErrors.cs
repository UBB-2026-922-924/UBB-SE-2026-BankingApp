namespace BankingApp.Domain.Common.Errors;

using ErrorOr;

public static class BeneficiaryErrors
{
    public static readonly Error NotFound =
        Error.NotFound("beneficiary.not_found", "Beneficiary was not found.");

    public static readonly Error Forbidden =
        Error.Forbidden("beneficiary.forbidden", "You do not have access to this beneficiary.");

    public static readonly Error InvalidIban =
        Error.Validation("beneficiary.invalid_iban", "The beneficiary IBAN is invalid.");

    public static readonly Error Duplicate =
        Error.Conflict("beneficiary.duplicate", "A beneficiary with this IBAN already exists.");
}
