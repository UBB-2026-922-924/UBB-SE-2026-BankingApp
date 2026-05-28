namespace BankingApp.Infrastructure.Persistence.Repositories;

using Domain.Aggregates.BillPaymentAggregate;
using BankingApp.Domain.Repositories;
using Data;
using Microsoft.EntityFrameworkCore;

public sealed class BillPaymentRepository(AppDbContext dbContext) : IBillPaymentRepository
{
    public async Task<BillPayment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.BillPayments.FirstOrDefaultAsync(billPayment => billPayment.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<BillPayment>> ListByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.BillPayments
            .Where(billPayment => billPayment.UserId == userId)
            .OrderByDescending(billPayment => billPayment.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(BillPayment billPayment, CancellationToken cancellationToken = default)
    {
        await dbContext.BillPayments.AddAsync(billPayment, cancellationToken);
    }
}
