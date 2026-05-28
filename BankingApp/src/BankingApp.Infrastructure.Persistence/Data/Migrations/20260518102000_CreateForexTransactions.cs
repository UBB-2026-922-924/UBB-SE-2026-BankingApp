namespace BankingApp.Infrastructure.Persistence.Data.Migrations;

using System;
using Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20260518102000_CreateForexTransactions")]
public partial class CreateForexTransactions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
                        name: "ForexTransactions",
                        columns: table => new
                        {
                            Id = table.Column<int>(type: "int", nullable: false)
                                .Annotation("SqlServer:Identity", "1, 1"),
                            UserId = table.Column<int>(type: "int", nullable: false),
                            SourceAccountId = table.Column<int>(type: "int", nullable: false),
                            TargetAccountId = table.Column<int>(type: "int", nullable: false),
                            SourceLedgerTransactionId = table.Column<int>(type: "int", nullable: true),
                            TargetLedgerTransactionId = table.Column<int>(type: "int", nullable: true),
                            SourceAmount = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                            TargetAmount = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                            ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                            Commission = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                            RateLockedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                            Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false, defaultValue: "Pending"),
                            CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                        },
                        constraints: table => table.PrimaryKey("PK_ForexTransactions", x => x.Id));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ForexTransactions");
    }
}
