namespace BankingApp.Infrastructure.Persistence.Data.Migrations;

using System;
using Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20260518102600_CreateNotifications")]
public partial class CreateNotifications : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
                        name: "Notifications",
                        columns: table => new
                        {
                            Id = table.Column<int>(type: "int", nullable: false)
                                .Annotation("SqlServer:Identity", "1, 1"),
                            UserId = table.Column<int>(type: "int", nullable: false),
                            Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                            Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                            Type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                            Channel = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                            IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                            RelatedEntityType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                            RelatedEntityId = table.Column<int>(type: "int", nullable: true),
                            CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                        },
                        constraints: table =>
                        {
                            table.PrimaryKey("PK_Notifications", x => x.Id);
                            table.ForeignKey(
                                name: "FK_Notifications_Users_UserId",
                                column: x => x.UserId,
                                principalTable: "Users",
                                principalColumn: "Id",
                                onDelete: ReferentialAction.Cascade);
                        });

        migrationBuilder.CreateIndex(
                        name: "IX_Notifications_UserId",
                        table: "Notifications",
                        column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Notifications");
    }
}
