namespace BankingApp.Infrastructure.Persistence.Data.Migrations;

using System;
using Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20260518102400_CreateTransfers")]
public partial class CreateTransfers : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
                        name: "Transfers",
                        columns: table => new
                        {
                            Id = table.Column<int>(type: "int", nullable: false)
                                .Annotation("SqlServer:Identity", "1, 1"),
                            UserId = table.Column<int>(type: "int", nullable: false),
                            SourceAccountId = table.Column<int>(type: "int", nullable: false),
                            LedgerTransactionId = table.Column<int>(type: "int", nullable: true),
                            RecipientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                            RecipientIban = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: false),
                            RecipientBankName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                            Amount = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                            ConvertedAmount = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                            ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                            Fee = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                            Reference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                            Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false, defaultValue: "Pending"),
                            EstimatedArrival = table.Column<DateTime>(type: "datetime2", nullable: true),
                            CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                        },
                        constraints: table => table.PrimaryKey("PK_Transfers", x => x.Id));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Transfers");
    }
}
