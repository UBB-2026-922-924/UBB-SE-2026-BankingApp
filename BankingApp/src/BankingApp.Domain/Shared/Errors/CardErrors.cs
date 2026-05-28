namespace BankingApp.Domain.Common.Errors;

using ErrorOr;

public static class CardErrors
{
    public static readonly Error NotFound =
        Error.NotFound("card.not_found", "Card was not found.");

    public static readonly Error AlreadyCancelled =
        Error.Conflict("card.already_cancelled", "Card has already been cancelled.");

    public static readonly Error AlreadyFrozen =
        Error.Conflict("card.already_frozen", "Card is already frozen.");

    public static readonly Error NotFrozen =
        Error.Conflict("card.not_frozen", "Card is not frozen.");
}
