namespace BankingApp.Domain.Aggregates.AccountAggregate.Entities;

using Common.Primitives;

public sealed class TransactionCategoryOverride : Entity<int>
{
    private TransactionCategoryOverride()
    {
    }

    public int TransactionId { get; private set; }

    public int UserId { get; private set; }

    public int CategoryId { get; private set; }

    public static TransactionCategoryOverride Create(int transactionId, int userId, int categoryId)
    {
        return new TransactionCategoryOverride
        {
            TransactionId = transactionId,
            UserId = userId,
            CategoryId = categoryId
        };
    }
}
