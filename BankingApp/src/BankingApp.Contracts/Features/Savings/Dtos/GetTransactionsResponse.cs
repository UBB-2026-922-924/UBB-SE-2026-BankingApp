namespace BankingApp.Contracts.Features.Savings.Dtos;

using System.Collections.Generic;
using BankingApp.Domain.Aggregates.SavingsAggregate.Entities;

public class GetTransactionsResponse
{
    public List<SavingsTransaction> Items { get; set; } = [];

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
