namespace BankingApp.Infrastructure.Persistence.Data.Migrations;

using System;
using Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20260518102300_CreateSavedBillers")]
public partial class CreateSavedBillers : Migration
{
    private static readonly string[] _savedBillersUserIdBillerIdColumns = ["UserId", "BillerId"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
                        name: "SavedBillers",
                        columns: table => new
                        {
                            Id = table.Column<int>(type: "int", nullable: false)
                                .Annotation("SqlServer:Identity", "1, 1"),
                            UserId = table.Column<int>(type: "int", nullable: false),
                            BillerId = table.Column<int>(type: "int", nullable: false),
                            Nickname = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                            DefaultReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                            CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                        },
                        constraints: table => table.PrimaryKey("PK_SavedBillers", x => x.Id));

        migrationBuilder.CreateIndex(
                        name: "IX_SavedBillers_UserId_BillerId",
                        table: "SavedBillers",
                        columns: _savedBillersUserIdBillerIdColumns,
                        unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "SavedBillers");
    }
}
