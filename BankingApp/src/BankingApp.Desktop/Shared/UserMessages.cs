namespace BankingApp.Desktop.Shared;

/// <summary>
///     Centralises all user-facing message strings shown by ViewModels.
///     Keeps display text out of call sites and in one place so it is easy to review, update,
///     or replace with a proper localisation layer later.
/// </summary>
internal static class UserMessages
{
    internal static class Dashboard
    {
        internal const string SessionExpired = "Your session expired. Please sign in again.";
        internal const string NotFound = "Dashboard data was not found for this account.";
        internal const string LoadFailed = "We couldn't load your dashboard. Please try again.";
        internal const string IncompleteResponse = "The dashboard response was incomplete.";
    }

    internal static class Register
    {
        internal const string EmailAlreadyExists = "This email is already registered.";
        internal const string InvalidEmail = "Please enter a valid email address.";
        internal const string WeakPassword =
            "Password must be at least 8 characters with uppercase, lowercase, a digit and a special character.";
        internal const string PasswordMismatch = "Passwords do not match.";
        internal const string AllFieldsRequired = "Please fill in all fields.";
    }

    internal static class Profile
    {
        internal const string NoPhoneNumber = "No phone number set";
    }

    internal static class Security
    {
        internal const string MinimumLengthRequired = "Minimum 8 characters required.";
        internal const string PasswordMismatch = "Passwords do not match.";
        internal const string IncorrectPassword = "Current password is incorrect.";
        internal const string UnexpectedError = "An unexpected error occurred.";
    }

    internal static class ForgotPassword
    {
        internal const string EmailRequired = "Please enter your email address.";
        internal const string AllFieldsRequired = "Please fill in all fields.";
        internal const string PasswordTooWeak =
            "Password must be at least 8 characters with uppercase, lowercase, a digit, and a special character.";
        internal const string CodeRequired = "Please paste the recovery code first.";
    }

    internal static class Exchange
    {
        internal const string CurrencyRequired = "Please select both source and target currencies.";
        internal const string SameCurrency = "Source and target currencies must be different.";
        internal const string AccountRequired = "Please select both source and target accounts.";
        internal const string AmountRequired = "Please enter an amount greater than zero.";
        internal const string PreviewFailed = "Could not load the exchange rate preview. Please try again.";
        internal const string ExecuteFailed = "The exchange could not be completed. Please try again.";
        internal const string HistoryLoadFailed = "Could not load exchange history. Please try again.";
    }

    internal static class RateAlerts
    {
        internal const string LoadFailed = "Could not load your rate alerts. Please try again.";
        internal const string CurrencyRequired = "Base currency and target currency are required.";
        internal const string CurrenciesMustDiffer = "Base currency and target currency must be different.";
        internal const string InvalidNumberFormat = "Invalid number format.";
        internal const string InvalidTargetRate = "Target rate must be a valid number (e.g. 1,2 or 1.2).";
        internal const string CreateFailed = "Could not create the rate alert. Please try again.";
        internal const string DeleteFailed = "Could not delete the rate alert. Please try again.";
    }

    internal static class TransferHistory
    {
        internal const string LoadFailed = "Could not load your transfer history. Please try again.";
        internal const string NoTransfers = "You haven't made any transfers yet.";
    }

    internal static class Transfer
    {
        internal const string InvalidIban = "Invalid IBAN format.";
        internal const string AmountMustBePositive = "The amount must be greater than 0.";
        internal const string NoAccountSelected = "No account selected.";
        internal const string AccountLoadFailed = "Could not load your accounts. Please try again.";
        internal const string TransferFailed = "Transfer failed. Please try again.";
    }
}
