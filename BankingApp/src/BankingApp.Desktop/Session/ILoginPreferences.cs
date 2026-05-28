namespace BankingApp.Desktop.Session;

/// <summary>Persists user login preferences across application launches.</summary>
public interface ILoginPreferences
{
    /// <summary>Gets the previously remembered email address, if any.</summary>
    public string? SavedEmail { get; }

    /// <summary>Gets a value indicating whether the user previously opted to be remembered.</summary>
    public bool RememberMe { get; }

    /// <summary>Saves or clears the remembered email based on the user's preference.</summary>
    public void Save(string email, bool rememberMe);

    /// <summary>Clears any saved login preferences.</summary>
    public void Clear();
}
