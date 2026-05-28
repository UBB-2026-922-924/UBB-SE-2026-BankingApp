namespace BankingApp.Infrastructure.Persistence.Data.Migrations;

using System;
using Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20260518101700_CreateBeneficiaries")]
public partial class CreateBeneficiaries : Migration
{
    private static readonly string[] _beneficiariesUserIdIbanColumns = ["UserId", "Iban"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
                        name: "Beneficiaries",
                        columns: table => new
                        {
                            Id = table.Column<int>(type: "int", nullable: false)
                                .Annotation("SqlServer:Identity", "1, 1"),
                            UserId = table.Column<int>(type: "int", nullable: false),
                            Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                            Iban = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: false),
                            BankName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                            LastTransferDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                            TotalAmountSent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                            TransferCount = table.Column<int>(type: "int", nullable: false),
                            CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                        },
                        constraints: table => table.PrimaryKey("PK_Beneficiaries", x => x.Id));

        migrationBuilder.CreateIndex(
                        name: "IX_Beneficiaries_UserId_Iban",
                        table: "Beneficiaries",
                        columns: _beneficiariesUserIdIbanColumns,
                        unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Beneficiaries");
    }
}
