namespace BankingApp.Domain.Common.Errors;

using ErrorOr;

/// <summary>
///     Canonical error definitions for biller directory and saved-biller operations.
/// </summary>
public static class BillerErrors
{
    /// <summary>The requested biller was not found.</summary>
    public static readonly Error BillerNotFound =
        Error.NotFound("biller_not_found", "Biller not found.");

    /// <summary>The requested saved biller was not found.</summary>
    public static readonly Error SavedBillerNotFound =
        Error.NotFound("saved_biller_not_found", "Saved biller not found.");

    /// <summary>The biller is already saved by the user.</summary>
    public static readonly Error BillerAlreadySaved =
        Error.Conflict("biller_already_saved", "This biller is already in your saved list.");
}
