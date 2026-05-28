namespace BankingApp.Infrastructure.Persistence.Data.Migrations;

using System;
using Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20260518102500_CreateNotificationPreferences")]
public partial class CreateNotificationPreferences : Migration
{
    private static readonly string[] _notificationPreferencesUserIdCategoryColumns = ["UserId", "Category"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
                        name: "NotificationPreferences",
                        columns: table => new
                        {
                            Id = table.Column<int>(type: "int", nullable: false)
                                .Annotation("SqlServer:Identity", "1, 1"),
                            UserId = table.Column<int>(type: "int", nullable: false),
                            Category = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                            PushEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                            EmailEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                            SmsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                            MinAmountThreshold = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                        },
                        constraints: table =>
                        {
                            table.PrimaryKey("PK_NotificationPreferences", x => x.Id);
                            table.ForeignKey(
                                name: "FK_NotificationPreferences_Users_UserId",
                                column: x => x.UserId,
                                principalTable: "Users",
                                principalColumn: "Id",
                                onDelete: ReferentialAction.Cascade);
                        });

        migrationBuilder.CreateIndex(
                        name: "IX_NotificationPreferences_UserId_Category",
                        table: "NotificationPreferences",
                        columns: _notificationPreferencesUserIdCategoryColumns,
                        unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "NotificationPreferences");
    }
}
