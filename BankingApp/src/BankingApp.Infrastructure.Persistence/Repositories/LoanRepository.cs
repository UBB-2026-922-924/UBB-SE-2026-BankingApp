namespace BankingApp.Infrastructure.Persistence.Repositories;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data;
using Domain.Aggregates.LoanAggregate;
using Domain.Aggregates.LoanAggregate.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

public sealed class LoanRepository(AppDbContext dbContext) : ILoanRepository
{
    public async Task<IReadOnlyCollection<Loan>> GetAllLoansAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Loans
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Loan?> GetLoanByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await dbContext.Loans
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Loan>> GetLoansByUserAsync(int userId, CancellationToken cancellationToken)
    {
        return await dbContext.Loans
            .AsNoTracking()
            .Where(l => l.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Loan>> GetLoansByStatusAsync(LoanStatus loanStatus, CancellationToken cancellationToken)
    {
        return await dbContext.Loans
            .AsNoTracking()
            .Where(l => l.LoanStatus == loanStatus)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Loan>> GetLoansByTypeAsync(LoanType loanType, CancellationToken cancellationToken)
    {
        return await dbContext.Loans
            .AsNoTracking()
            .Where(l => l.LoanType == loanType)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveAmortizationAsync(IReadOnlyCollection<AmortizationRow> rows, CancellationToken cancellationToken)
    {
        dbContext.AmortizationRows.AddRange(rows);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<AmortizationRow>> GetAmortizationAsync(int loanId, CancellationToken cancellationToken)
    {
        List<AmortizationRow> rows = await dbContext.AmortizationRows
            .AsNoTracking()
            .Where(r => r.LoanId == loanId)
            .OrderBy(r => r.InstallmentNumber)
            .ToListAsync(cancellationToken);

        MarkCurrentRow(rows);
        return rows;
    }

    public async Task<int> CreateLoanApplicationAsync(LoanApplication application, CancellationToken cancellationToken)
    {
        dbContext.LoanApplications.Add(application);
        await dbContext.SaveChangesAsync(cancellationToken);
        return application.Id;
    }

    public async Task UpdateLoanApplicationStatusAsync(int id, LoanApplicationStatus loanApplicationStatus, string? reason, CancellationToken cancellationToken)
    {
        LoanApplication? application = await dbContext.LoanApplications
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (application is null)
        {
            return;
        }

        if (loanApplicationStatus == LoanApplicationStatus.Approved)
        {
            application.Approve();
        }
        else if (loanApplicationStatus == LoanApplicationStatus.Rejected)
        {
            application.Reject(reason ?? string.Empty);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CreateLoanAsync(Loan loan, CancellationToken cancellationToken)
    {
        dbContext.Loans.Add(loan);
        await dbContext.SaveChangesAsync(cancellationToken);
        return loan.Id;
    }

    public async Task UpdateLoanAfterPaymentAsync(int id, decimal newBalance, int newRemainingMonths, LoanStatus newStatus, CancellationToken cancellationToken)
    {
        Loan? loan = await dbContext.Loans.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (loan is null)
        {
            return;
        }

        loan.PayInstallment(loan.MonthlyInstallment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void MarkCurrentRow(List<AmortizationRow> rows)
    {
        bool isCurrentSet = false;
        foreach (AmortizationRow row in rows)
        {
            if (!isCurrentSet && row.DueDate.Date >= System.DateTime.Today)
            {
                row.MarkAsCurrent();
                isCurrentSet = true;
            }
            else
            {
                row.ClearCurrent();
            }
        }
    }
}
