namespace BankingApp.Infrastructure.Persistence.Data.Migrations;

using System;
using Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20260518101800_CreateBillers")]
public partial class CreateBillers : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
                        name: "Billers",
                        columns: table => new
                        {
                            Id = table.Column<int>(type: "int", nullable: false)
                                .Annotation("SqlServer:Identity", "1, 1"),
                            Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                            Category = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                            LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                            IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                        },
                        constraints: table => table.PrimaryKey("PK_Billers", x => x.Id));

        migrationBuilder.CreateIndex(
                        name: "IX_Billers_Name",
                        table: "Billers",
                        column: "Name",
                        unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Billers");
    }
}
