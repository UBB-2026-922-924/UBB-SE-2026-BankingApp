namespace BankingApp.Desktop.State;

/// <summary>Stores short-lived transfer draft details shared between desktop views.</summary>
public interface ITransferDraftState
{
    /// <summary>Gets a value indicating whether the current draft contains beneficiary data.</summary>
    public bool HasDraft { get; }

    /// <summary>Gets the prefilled recipient name.</summary>
    public string RecipientName { get; }

    /// <summary>Gets the prefilled recipient IBAN.</summary>
    public string RecipientIban { get; }

    /// <summary>Stores the provided recipient details for the next transfer flow.</summary>
    public void SetDraft(string recipientName, string recipientIban);

    /// <summary>Clears the current transfer draft.</summary>
    public void Clear();
}
