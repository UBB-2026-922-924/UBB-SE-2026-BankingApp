namespace BankingApp.Domain.Aggregates.UserAggregate.Entities;

using Common.Primitives;

public sealed class UserCardPreference : Entity<int>
{
    private UserCardPreference()
    {
    }

    public string SortOption { get; private set; } = string.Empty;

    public DateTime UpdatedAt { get; private set; }

    public static UserCardPreference Create(
        int userId,
        string sortOption,
        DateTime updatedAt)
    {
        return new UserCardPreference
        {
            Id = userId, // UserId acts as the PK
            SortOption = sortOption,
            UpdatedAt = updatedAt
        };
    }
}
