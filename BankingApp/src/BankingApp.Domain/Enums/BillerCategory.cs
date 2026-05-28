namespace BankingApp.Domain.Enums;

/// <summary>
///     Classifies a biller into a business sector category.
/// </summary>
public enum BillerCategory
{
    /// <summary>
    ///     Electricity, water, gas, and other utility providers.
    /// </summary>
    Utilities,

    /// <summary>
    ///     Mobile, internet, and telephone operators.
    /// </summary>
    Telecom,

    /// <summary>
    ///     Life, health, vehicle, and property insurance companies.
    /// </summary>
    Insurance,

    /// <summary>
    ///     Landlords and property management companies collecting rent.
    /// </summary>
    Rent,

    /// <summary>
    ///     State taxes, fines, and government fee collectors.
    /// </summary>
    Government,

    /// <summary>
    ///     Streaming platforms, software licenses, and other subscription services.
    /// </summary>
    Subscriptions,

    /// <summary>
    ///     Any biller that does not fit the specific categories above.
    /// </summary>
    Other
}