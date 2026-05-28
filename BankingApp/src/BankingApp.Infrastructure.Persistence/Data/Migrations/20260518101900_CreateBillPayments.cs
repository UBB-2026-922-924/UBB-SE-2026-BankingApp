namespace BankingApp.Infrastructure.Persistence.Data.Migrations;

using System;
using Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20260518101900_CreateBillPayments")]
public partial class CreateBillPayments : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
                        name: "BillPayments",
                        columns: table => new
                        {
                            Id = table.Column<int>(type: "int", nullable: false)
                                .Annotation("SqlServer:Identity", "1, 1"),
                            UserId = table.Column<int>(type: "int", nullable: false),
                            SourceAccountId = table.Column<int>(type: "int", nullable: false),
                            BillerId = table.Column<int>(type: "int", nullable: false),
                            LedgerTransactionId = table.Column<int>(type: "int", nullable: true),
                            BillerReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                            Amount = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                            Fee = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                            ReceiptNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                            Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false, defaultValue: "Pending"),
                            CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                        },
                        constraints: table => table.PrimaryKey("PK_BillPayments", x => x.Id));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "BillPayments");
    }
}
