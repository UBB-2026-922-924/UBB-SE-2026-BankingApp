namespace BankingApp.Infrastructure.Persistence.Data.Migrations;

using System;
using Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20260518101000_CreateUsers")]
public partial class CreateUsers : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
                        name: "Users",
                        columns: table => new
                        {
                            Id = table.Column<int>(type: "int", nullable: false)
                                .Annotation("SqlServer:Identity", "1, 1"),
                            Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                            FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                            PhoneNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                            DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                            Address = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                            Nationality = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                            PreferredLanguage = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                            CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                            UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                        },
                        constraints: table => table.PrimaryKey("PK_Users", x => x.Id));

        migrationBuilder.CreateIndex(
                        name: "IX_Users_Email",
                        table: "Users",
                        column: "Email",
                        unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Users");
    }
}
