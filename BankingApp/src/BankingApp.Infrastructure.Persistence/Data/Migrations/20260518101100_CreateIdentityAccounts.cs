namespace BankingApp.Infrastructure.Persistence.Data.Migrations;

using System;
using Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20260518101100_CreateIdentityAccounts")]
public partial class CreateIdentityAccounts : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
                        name: "IdentityAccounts",
                        columns: table => new
                        {
                            Id = table.Column<int>(type: "int", nullable: false)
                                .Annotation("SqlServer:Identity", "1, 1"),
                            UserId = table.Column<int>(type: "int", nullable: false),
                            PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                            IsLocked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                            LockoutEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                            FailedLoginAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                        },
                        constraints: table => table.PrimaryKey("PK_IdentityAccounts", x => x.Id));

        migrationBuilder.CreateIndex(
                        name: "IX_IdentityAccounts_UserId",
                        table: "IdentityAccounts",
                        column: "UserId",
                        unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "IdentityAccounts");
    }
}
