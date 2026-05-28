namespace BankingApp.Infrastructure.Persistence.Data.Migrations;

using System;
using Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20260518101400_CreateAccounts")]
public partial class CreateAccounts : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
                        name: "Accounts",
                        columns: table => new
                        {
                            Id = table.Column<int>(type: "int", nullable: false)
                                .Annotation("SqlServer:Identity", "1, 1"),
                            UserId = table.Column<int>(type: "int", nullable: false),
                            AccountName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                            Iban = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: false),
                            Balance = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                            AccountType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                            Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false, defaultValue: "Active"),
                            CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                        },
                        constraints: table => table.PrimaryKey("PK_Accounts", x => x.Id));

        migrationBuilder.CreateIndex(
                        name: "IX_Accounts_Iban",
                        table: "Accounts",
                        column: "Iban",
                        unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Accounts");
    }
}
