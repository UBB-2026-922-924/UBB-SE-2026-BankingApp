namespace BankingApp.Infrastructure.Persistence.Data.Migrations;

using System;
using Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20260518101600_CreateTransactions")]
public partial class CreateTransactions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
                        name: "Transactions",
                        columns: table => new
                        {
                            Id = table.Column<int>(type: "int", nullable: false)
                                .Annotation("SqlServer:Identity", "1, 1"),
                            AccountId = table.Column<int>(type: "int", nullable: false),
                            CardId = table.Column<int>(type: "int", nullable: true),
                            TransactionRef = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                            Type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                            Direction = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                            Amount = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                            BalanceAfter = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                            CounterpartyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                            CounterpartyIban = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: true),
                            MerchantName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                            CategoryId = table.Column<int>(type: "int", nullable: true),
                            Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                            Fee = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                            ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                            Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                            RelatedEntityType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                            RelatedEntityId = table.Column<int>(type: "int", nullable: true),
                            CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                        },
                        constraints: table =>
                        {
                            table.PrimaryKey("PK_Transactions", x => x.Id);
                            table.ForeignKey(
                                name: "FK_Transactions_Accounts_AccountId",
                                column: x => x.AccountId,
                                principalTable: "Accounts",
                                principalColumn: "Id",
                                onDelete: ReferentialAction.Cascade);
                        });

        migrationBuilder.CreateIndex(
                        name: "IX_Transactions_AccountId",
                        table: "Transactions",
                        column: "AccountId");

        migrationBuilder.CreateIndex(
                        name: "IX_Transactions_TransactionRef",
                        table: "Transactions",
                        column: "TransactionRef",
                        unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Transactions");
    }
}
