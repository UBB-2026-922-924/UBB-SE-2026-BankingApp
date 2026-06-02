using BankingApp.Domain.Aggregates.SavingsAggregate;

namespace BankingApp.Contracts.Features.Savings.Dtos
{
    public class GetTransactionsResponse
    {
        public List<SavingsTransaction> Items { get; set; } = new ();

        public int TotalCount { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }
    }
}
