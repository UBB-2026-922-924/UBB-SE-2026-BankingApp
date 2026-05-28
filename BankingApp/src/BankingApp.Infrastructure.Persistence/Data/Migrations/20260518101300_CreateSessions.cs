namespace BankingApp.Infrastructure.Persistence.Data.Migrations;

using System;
using Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20260518101300_CreateSessions")]
public partial class CreateSessions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
                        name: "Sessions",
                        columns: table => new
                        {
                            Id = table.Column<int>(type: "int", nullable: false)
                                .Annotation("SqlServer:Identity", "1, 1"),
                            IdentityAccountId = table.Column<int>(type: "int", nullable: false),
                            Token = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                            DeviceInfo = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                            Browser = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                            IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                            LastActiveAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                            ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                            IsRevoked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                            CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                        },
                        constraints: table =>
                        {
                            table.PrimaryKey("PK_Sessions", x => x.Id);
                            table.ForeignKey(
                                name: "FK_Sessions_IdentityAccounts_IdentityAccountId",
                                column: x => x.IdentityAccountId,
                                principalTable: "IdentityAccounts",
                                principalColumn: "Id",
                                onDelete: ReferentialAction.Cascade);
                        });

        migrationBuilder.CreateIndex(
                        name: "IX_Sessions_IdentityAccountId",
                        table: "Sessions",
                        column: "IdentityAccountId");

        migrationBuilder.CreateIndex(
                        name: "IX_Sessions_Token",
                        table: "Sessions",
                        column: "Token",
                        unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Sessions");
    }
}
