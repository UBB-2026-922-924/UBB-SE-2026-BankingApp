namespace BankingApp.Contracts.Features.Statistics.Dtos;

using System;

public class BalanceTrendPointDto
{
    public DateTime Date { get; set; }

    public decimal Balance { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not BalanceTrendPointDto other)
        {
            return false;
        }

        return Date.Date == other.Date.Date && Balance == other.Balance;
    }

    public override int GetHashCode() => HashCode.Combine(Date.Date, Balance);
}
