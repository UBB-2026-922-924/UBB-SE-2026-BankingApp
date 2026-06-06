using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankingApp.Infrastructure.Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class UnifiedSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Is2FAEnabled",
                table: "IdentityAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Preferred2FAMethod",
                table: "IdentityAccounts",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatSession",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    Feedback = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatSession", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Loan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoanType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Principal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OutstandingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(6,4)", nullable: false),
                    MonthlyInstallment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RemainingMonths = table.Column<int>(type: "int", nullable: false),
                    LoanStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TermInMonths = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoanApplication",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoanType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DesiredAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreferredTermMonths = table.Column<int>(type: "int", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ApplicationStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanApplication", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoanEstimates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IndicativeRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MonthlyInstallment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalRepayable = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanEstimates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OAuthLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProviderUserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ProviderEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LinkedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OAuthLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OAuthLinks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResetTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Portfolio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Portfolio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SavingsAccount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SavingsType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AccruedInterest = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AnnualPercentageYield = table.Column<decimal>(type: "decimal(6,4)", nullable: false),
                    MaturityDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AccountStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AccountName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FundingAccountId = table.Column<int>(type: "int", nullable: true),
                    TargetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TargetDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingsAccount", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionCategoryOverrides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionCategoryOverrides", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserCardPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    SortOption = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCardPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCardPreferences_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChatSessionId = table.Column<int>(type: "int", nullable: false),
                    Sender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessage_ChatSession_ChatSessionId",
                        column: x => x.ChatSessionId,
                        principalTable: "ChatSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AmortizationRow",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanId = table.Column<int>(type: "int", nullable: false),
                    InstallmentNumber = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PrincipalPortion = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestPortion = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RemainingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmortizationRow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AmortizationRow_Loan_LoanId",
                        column: x => x.LoanId,
                        principalTable: "Loan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvestmentHolding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortfolioId = table.Column<int>(type: "int", nullable: false),
                    Ticker = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AssetType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    AvgPurchasePrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnrealizedGainLoss = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentHolding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestmentHolding_Portfolio_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AutoDeposit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SavingsAccountId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NextRunDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SourceAccountId = table.Column<int>(type: "int", nullable: true),
                    DayOfMonth = table.Column<int>(type: "int", nullable: true),
                    DayOfWeek = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoDeposit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutoDeposit_SavingsAccount_SavingsAccountId",
                        column: x => x.SavingsAccountId,
                        principalTable: "SavingsAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavingsTransaction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SavingsAccountId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingsTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavingsTransaction_SavingsAccount_SavingsAccountId",
                        column: x => x.SavingsAccountId,
                        principalTable: "SavingsAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatAttachment",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    messageId = table.Column<int>(type: "int", nullable: false),
                    attachmentName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    fileType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    fileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    storageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatAttachment", x => x.id);
                    table.ForeignKey(
                        name: "FK_ChatAttachment_ChatMessage_messageId",
                        column: x => x.messageId,
                        principalTable: "ChatMessage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvestmentTransaction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoldingId = table.Column<int>(type: "int", nullable: false),
                    Ticker = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    PricePerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Fees = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OrderType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvestmentHoldingId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestmentTransaction_InvestmentHolding_InvestmentHoldingId",
                        column: x => x.InvestmentHoldingId,
                        principalTable: "InvestmentHolding",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AmortizationRow_LoanId",
                table: "AmortizationRow",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_AutoDeposit_SavingsAccountId",
                table: "AutoDeposit",
                column: "SavingsAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatAttachment_messageId",
                table: "ChatAttachment",
                column: "messageId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessage_ChatSessionId",
                table: "ChatMessage",
                column: "ChatSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSession_UserId",
                table: "ChatSession",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentHolding_PortfolioId_Ticker",
                table: "InvestmentHolding",
                columns: new[] { "PortfolioId", "Ticker" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentTransaction_HoldingId",
                table: "InvestmentTransaction",
                column: "HoldingId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentTransaction_InvestmentHoldingId",
                table: "InvestmentTransaction",
                column: "InvestmentHoldingId");

            migrationBuilder.CreateIndex(
                name: "IX_Loan_UserId",
                table: "Loan",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplication_UserId",
                table: "LoanApplication",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OAuthLinks_Provider_ProviderUserId",
                table: "OAuthLinks",
                columns: new[] { "Provider", "ProviderUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OAuthLinks_UserId",
                table: "OAuthLinks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_TokenHash",
                table: "PasswordResetTokens",
                column: "TokenHash");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId",
                table: "PasswordResetTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Portfolio_UserId",
                table: "Portfolio",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavingsAccount_UserId",
                table: "SavingsAccount",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingsTransaction_SavingsAccountId",
                table: "SavingsTransaction",
                column: "SavingsAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCategoryOverrides_CategoryId",
                table: "TransactionCategoryOverrides",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCategoryOverrides_TransactionId",
                table: "TransactionCategoryOverrides",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCategoryOverrides_UserId",
                table: "TransactionCategoryOverrides",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AmortizationRow");

            migrationBuilder.DropTable(
                name: "AutoDeposit");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "ChatAttachment");

            migrationBuilder.DropTable(
                name: "InvestmentTransaction");

            migrationBuilder.DropTable(
                name: "LoanApplication");

            migrationBuilder.DropTable(
                name: "LoanEstimates");

            migrationBuilder.DropTable(
                name: "OAuthLinks");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "SavingsTransaction");

            migrationBuilder.DropTable(
                name: "TransactionCategoryOverrides");

            migrationBuilder.DropTable(
                name: "UserCardPreferences");

            migrationBuilder.DropTable(
                name: "Loan");

            migrationBuilder.DropTable(
                name: "ChatMessage");

            migrationBuilder.DropTable(
                name: "InvestmentHolding");

            migrationBuilder.DropTable(
                name: "SavingsAccount");

            migrationBuilder.DropTable(
                name: "ChatSession");

            migrationBuilder.DropTable(
                name: "Portfolio");

            migrationBuilder.DropColumn(
                name: "Is2FAEnabled",
                table: "IdentityAccounts");

            migrationBuilder.DropColumn(
                name: "Preferred2FAMethod",
                table: "IdentityAccounts");
        }
    }
}
