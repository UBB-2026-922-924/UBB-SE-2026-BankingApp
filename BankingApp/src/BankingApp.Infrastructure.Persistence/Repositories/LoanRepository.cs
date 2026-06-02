namespace BankingApp.Infrastructure.Persistence.Repositories;

using System.Data;
using System.Data.Common;
using System.Globalization;
using Contracts.Features.Loans.Dtos;
using Data;
using Domain.Enums;
using Domain.Aggregates.LoanAggregate;
using Domain.Aggregates.LoanAggregate.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

/// <summary>
/// EF Core-backed repository for loans and loan applications.
/// </summary>
public class LoanRepository : ILoanRepository
{
    private const int EmptyCount = 0;
    private const int FirstIndex = 0;

    private readonly AppDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoanRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The application's EF Core database _dbContext.</param>
    public LoanRepository(AppDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    /// <summary>
    /// Retrieves all loans from storage using the Loan DbSet.
    /// </summary>
    public async Task<List<Loan>> GetAllLoansAsync()
    {
        return await ReadLoansAsync();
    }

    /// <summary>
    /// Retrieves a loan by its identifier using EF Core.
    /// </summary>
    public async Task<Loan?> GetLoanByIdAsync(int id)
    {
        return (await ReadLoansAsync("WHERE Id = @id", ("@id", DbType.Int32, id))).FirstOrDefault();
    }

    /// <summary>
    /// Retrieves loans belonging to the specified user through the user navigation mapping.
    /// </summary>
    public async Task<List<Loan>> GetLoansByUserAsync(int userId)
    {
        return await ReadLoansAsync("WHERE UserId = @userId", ("@userId", DbType.Int32, userId));
    }

    /// <summary>
    /// Retrieves loans filtered by type using LINQ.
    /// </summary>
    public async Task<List<Loan>> GetLoansByTypeAsync(LoanType loanType)
    {
        return await ReadLoansAsync("WHERE LoanType = @loanType", ("@loanType", DbType.Int32, (int)loanType));
    }

    /// <summary>
    /// Retrieves loans filtered by status using LINQ.
    /// </summary>
    public async Task<List<Loan>> GetLoansByStatusAsync(LoanStatus loanStatus)
    {
        return await ReadLoansAsync("WHERE LoanStatus = @loanStatus", ("@loanStatus", DbType.Int32, (int)loanStatus));
    }

    /// <summary>
    /// Saves an amortization schedule for a loan using EF Core entities.
    /// </summary>
    public async Task SaveAmortizationAsync(List<AmortizationRow> rows)
    {
        if (rows.Count == EmptyCount)
        {
            return;
        }

        await using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            int loanId = rows[FirstIndex].LoanId;
            var existingRows = _dbContext.AmortizationRows.Where(amortizationRow => amortizationRow.LoanId == loanId);
            _dbContext.AmortizationRows.RemoveRange(existingRows);
            await _dbContext.AmortizationRows.AddRangeAsync(rows);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception exception) when (
        exception is OperationCanceledException or DbUpdateException or DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Retrieves amortization rows for a loan using the EF Core DbSet.
    /// </summary>
    public async Task<List<AmortizationRow>> GetAmortizationAsync(int loanId)
    {
        var rows = await _dbContext.AmortizationRows
            .AsNoTracking()
            .Where(amortizationRow => amortizationRow.LoanId == loanId)
            .OrderBy(amortizationRow => amortizationRow.InstallmentNumber)
            .ToListAsync();

        MarkCurrentAmortizationRow(rows);
        return rows;
    }

    /// <summary>
    /// Creates a new loan application using EF Core.
    /// </summary>
    public async Task<int> CreateLoanApplicationAsync(LoanApplicationRequest application)
    {
        var loanApplication = LoanApplication.Create(
            application.UserId,
            application.LoanType,
            application.DesiredAmount,
            application.PreferredTermMonths,
            application.Purpose);

        _dbContext.LoanApplications.Add(loanApplication);
        await _dbContext.SaveChangesAsync();
        return loanApplication.Id;
    }

    /// <summary>
    /// Updates review status and optional rejection reason for an application using EF Core tracking.
    /// </summary>
    public async Task UpdateLoanApplicationStatusAsync(
        int applicationId,
        LoanApplicationStatus loanApplicationStatus,
        string? reason)
    {
        await ExecuteNonQueryAsync(
            """
            UPDATE LoanApplication
            SET ApplicationStatus = @status,
                RejectionReason = @reason
            WHERE Id = @applicationId
            """,
            ("@status", DbType.Int32, (int)loanApplicationStatus),
            ("@reason", DbType.String, (object?)reason ?? DBNull.Value),
            ("@applicationId", DbType.Int32, applicationId));
    }

    /// <summary>
    /// Creates a new loan record using the EF Core DbSet.
    /// </summary>
    public async Task<int> CreateLoanAsync(Loan loan)
    {
        _dbContext.Loans.Add(loan);
        await _dbContext.SaveChangesAsync();
        return loan.Id;
    }

    /// <summary>
    /// Updates a loan after a payment is processed using EF Core.
    /// </summary>
    public async Task UpdateLoanAfterPaymentAsync(
        int loanId,
        decimal newBalance,
        int newRemainingMonths,
        LoanStatus newStatus)
    {
        await ExecuteNonQueryAsync(
            """
            UPDATE Loan
            SET OutstandingBalance = @newBalance,
                RemainingMonths = @newRemainingMonths,
                LoanStatus = @newStatus
            WHERE Id = @loanId
            """,
            ("@newBalance", DbType.Decimal, newBalance),
            ("@newRemainingMonths", DbType.Int32, newRemainingMonths),
            ("@newStatus", DbType.Int32, (int)newStatus),
            ("@loanId", DbType.Int32, loanId));
    }

    private async Task<List<Loan>> ReadLoansAsync(
        string? whereClause = null,
        params (string Name, DbType Type, object Value)[] parameters)
    {
        DbConnection connection = _dbContext.Database.GetDbConnection();
        bool shouldCloseConnection = connection.State != ConnectionState.Open;

        if (shouldCloseConnection)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using DbCommand command = connection.CreateCommand();
            command.CommandText =
                $"""
                 SELECT
                     Id,
                     UserId,
                     LoanType,
                     Principal,
                     OutstandingBalance,
                     InterestRate,
                     MonthlyInstallment,
                     RemainingMonths,
                     LoanStatus,
                     TermInMonths,
                     StartDate
                 FROM Loan
                 {whereClause}
                 """;

            foreach ((string name, DbType type, object value) in parameters)
            {
                AddParameter(command, name, type, value);
            }

            await using DbDataReader reader = await command.ExecuteReaderAsync();
            var loans = new List<Loan>();

            while (await reader.ReadAsync())
            {
                loans.Add(Loan.Reconstitute(
                    ReadInt32(reader, "Id"),
                    ReadInt32(reader, "UserId"),
                    ReadEnum<LoanType>(reader, "LoanType"),
                    ReadDecimal(reader, "Principal"),
                    ReadDecimal(reader, "OutstandingBalance"),
                    ReadDecimal(reader, "InterestRate"),
                    ReadDecimal(reader, "MonthlyInstallment"),
                    ReadInt32(reader, "RemainingMonths"),
                    ReadEnum<LoanStatus>(reader, "LoanStatus"),
                    ReadInt32(reader, "TermInMonths"),
                    ReadDateTime(reader, "StartDate")));
            }

            return loans;
        }
        finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync();
            }
        }
    }

    private async Task ExecuteNonQueryAsync(
        string commandText,
        params (string Name, DbType Type, object Value)[] parameters)
    {
        DbConnection connection = _dbContext.Database.GetDbConnection();
        bool shouldCloseConnection = connection.State != ConnectionState.Open;

        if (shouldCloseConnection)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using DbCommand command = connection.CreateCommand();
            command.CommandText = commandText;

            foreach ((string name, DbType type, object value) in parameters)
            {
                AddParameter(command, name, type, value);
            }

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static void AddParameter(DbCommand command, string name, DbType type, object value)
    {
        DbParameter parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.DbType = type;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private static int ReadInt32(IDataRecord record, string columnName)
    {
        return Convert.ToInt32(record[columnName], CultureInfo.InvariantCulture);
    }

    private static decimal ReadDecimal(IDataRecord record, string columnName)
    {
        return Convert.ToDecimal(record[columnName], CultureInfo.InvariantCulture);
    }

    private static DateTime ReadDateTime(IDataRecord record, string columnName)
    {
        return Convert.ToDateTime(record[columnName], CultureInfo.InvariantCulture);
    }

    private static void MarkCurrentAmortizationRow(List<AmortizationRow> rows)
    {
        bool isCurrentSet = false;
        foreach (AmortizationRow row in rows)
        {
            if (!isCurrentSet && row.DueDate.Date >= DateTime.Today)
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

    private static TEnum ReadEnum<TEnum>(IDataRecord record, string columnName)
        where TEnum : struct, Enum
    {
        object value = record[columnName];

        if (value is string textValue)
        {
            if (Enum.TryParse<TEnum>(textValue, true, out TEnum enumByName))
            {
                return enumByName;
            }

            return (TEnum)Enum.ToObject(typeof(TEnum), Convert.ToInt32(textValue, CultureInfo.InvariantCulture));
        }

        return (TEnum)Enum.ToObject(typeof(TEnum), Convert.ToInt32(value, CultureInfo.InvariantCulture));
    }
}
