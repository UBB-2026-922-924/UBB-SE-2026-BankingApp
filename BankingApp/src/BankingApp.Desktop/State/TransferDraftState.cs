namespace BankingApp.Desktop.State;

/// <inheritdoc />
public sealed class TransferDraftState : ITransferDraftState
{
    /// <inheritdoc />
    public bool HasDraft => !string.IsNullOrWhiteSpace(RecipientIban);

    /// <inheritdoc />
    public string RecipientName { get; private set; } = string.Empty;

    /// <inheritdoc />
    public string RecipientIban { get; private set; } = string.Empty;

    /// <inheritdoc />
    public void SetDraft(string recipientName, string recipientIban)
    {
        RecipientName = recipientName;
        RecipientIban = recipientIban;
    }

    /// <inheritdoc />
    public void Clear()
    {
        RecipientName = string.Empty;
        RecipientIban = string.Empty;
    }
}
