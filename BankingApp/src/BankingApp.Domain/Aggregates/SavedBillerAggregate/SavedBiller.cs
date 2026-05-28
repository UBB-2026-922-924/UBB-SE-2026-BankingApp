namespace BankingApp.Domain.Aggregates.SavedBillerAggregate;

using Common.Primitives;

public sealed class SavedBiller : AggregateRoot<int>
{
    private SavedBiller()
    {
    }

    public int UserId { get; private set; }

    public int BillerId { get; private set; }

    public string? Nickname { get; private set; }

    public string? DefaultReference { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static SavedBiller Create(int userId, int billerId, string? nickname, string? defaultReference, DateTime createdAt)
    {
        return new SavedBiller
        {
            UserId = userId,
            BillerId = billerId,
            Nickname = nickname,
            DefaultReference = defaultReference,
            CreatedAt = createdAt
        };
    }

    public void Update(string? nickname, string? defaultReference)
    {
        Nickname = nickname;
        DefaultReference = defaultReference;
    }
}
