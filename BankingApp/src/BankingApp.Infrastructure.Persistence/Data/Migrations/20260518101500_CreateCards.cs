namespace BankingApp.Infrastructure.Persistence.Data.Migrations;

using System;
using Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20260518101500_CreateCards")]
public partial class CreateCards : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
                        name: "Cards",
                        columns: table => new
                        {
                            Id = table.Column<int>(type: "int", nullable: false)
                                .Annotation("SqlServer:Identity", "1, 1"),
                            AccountId = table.Column<int>(type: "int", nullable: false),
                            UserId = table.Column<int>(type: "int", nullable: false),
                            CardNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                            CardholderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                            ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                            Cvv = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                            CardType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                            CardBrand = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                            Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false, defaultValue: "Active"),
                            DailyTransactionLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                            MonthlySpendingCap = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                            AtmWithdrawalLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                            ContactlessLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                            IsContactlessEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                            IsOnlineEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                            SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                            CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                            CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                        },
                        constraints: table =>
                        {
                            table.PrimaryKey("PK_Cards", x => x.Id);
                            table.ForeignKey(
                                name: "FK_Cards_Accounts_AccountId",
                                column: x => x.AccountId,
                                principalTable: "Accounts",
                                principalColumn: "Id",
                                onDelete: ReferentialAction.Cascade);
                        });

        migrationBuilder.CreateIndex(
                        name: "IX_Cards_AccountId",
                        table: "Cards",
                        column: "AccountId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Cards");
    }
}
