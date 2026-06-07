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
            // Idempotent column additions — skip if the column already exists
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[IdentityAccounts]') AND name = 'Is2FAEnabled')
                    ALTER TABLE [IdentityAccounts] ADD [Is2FAEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[IdentityAccounts]') AND name = 'Preferred2FAMethod')
                    ALTER TABLE [IdentityAccounts] ADD [Preferred2FAMethod] nvarchar(64) NULL;");

            // Idempotent table creations — skip if the table already exists
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[Categories]') IS NULL
                CREATE TABLE [Categories] (
                    [Id] int NOT NULL IDENTITY,
                    [Name] nvarchar(100) NOT NULL,
                    [Icon] nvarchar(100) NULL,
                    [IsSystem] bit NOT NULL DEFAULT CAST(1 AS bit),
                    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
                );");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[ChatSession]') IS NULL
                CREATE TABLE [ChatSession] (
                    [Id] int NOT NULL IDENTITY,
                    [UserId] int NOT NULL,
                    [Subject] nvarchar(500) NOT NULL,
                    [Status] nvarchar(20) NOT NULL,
                    [Rating] int NULL,
                    [Feedback] nvarchar(2000) NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [UpdatedAt] datetime2 NOT NULL,
                    CONSTRAINT [PK_ChatSession] PRIMARY KEY ([Id])
                );");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[Loan]') IS NULL
                CREATE TABLE [Loan] (
                    [Id] int NOT NULL IDENTITY,
                    [UserId] int NOT NULL,
                    [LoanType] nvarchar(20) NOT NULL,
                    [Principal] decimal(18,2) NOT NULL,
                    [OutstandingBalance] decimal(18,2) NOT NULL,
                    [InterestRate] decimal(6,4) NOT NULL,
                    [MonthlyInstallment] decimal(18,2) NOT NULL,
                    [RemainingMonths] int NOT NULL,
                    [LoanStatus] nvarchar(20) NOT NULL,
                    [TermInMonths] int NOT NULL,
                    [StartDate] datetime2 NOT NULL,
                    CONSTRAINT [PK_Loan] PRIMARY KEY ([Id])
                );");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[LoanApplication]') IS NULL
                CREATE TABLE [LoanApplication] (
                    [Id] int NOT NULL IDENTITY,
                    [UserId] int NOT NULL,
                    [LoanType] nvarchar(20) NOT NULL,
                    [DesiredAmount] decimal(18,2) NOT NULL,
                    [PreferredTermMonths] int NOT NULL,
                    [Purpose] nvarchar(500) NOT NULL,
                    [ApplicationStatus] nvarchar(20) NOT NULL,
                    [RejectionReason] nvarchar(500) NULL,
                    CONSTRAINT [PK_LoanApplication] PRIMARY KEY ([Id])
                );");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[LoanEstimates]') IS NULL
                CREATE TABLE [LoanEstimates] (
                    [Id] int NOT NULL IDENTITY,
                    [IndicativeRate] decimal(18,4) NOT NULL,
                    [MonthlyInstallment] decimal(18,2) NOT NULL,
                    [TotalRepayable] decimal(18,2) NOT NULL,
                    CONSTRAINT [PK_LoanEstimates] PRIMARY KEY ([Id])
                );");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[OAuthLinks]') IS NULL
                CREATE TABLE [OAuthLinks] (
                    [Id] int NOT NULL IDENTITY,
                    [UserId] int NOT NULL,
                    [Provider] nvarchar(100) NOT NULL,
                    [ProviderUserId] nvarchar(256) NOT NULL,
                    [ProviderEmail] nvarchar(256) NULL,
                    [LinkedAt] datetime2 NOT NULL,
                    CONSTRAINT [PK_OAuthLinks] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_OAuthLinks_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
                );");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[PasswordResetTokens]') IS NULL
                CREATE TABLE [PasswordResetTokens] (
                    [Id] int NOT NULL IDENTITY,
                    [UserId] int NOT NULL,
                    [TokenHash] nvarchar(512) NOT NULL,
                    [ExpiresAt] datetime2 NOT NULL,
                    [UsedAt] datetime2 NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    CONSTRAINT [PK_PasswordResetTokens] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_PasswordResetTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
                );");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[Portfolio]') IS NULL
                CREATE TABLE [Portfolio] (
                    [Id] int NOT NULL IDENTITY,
                    [UserId] int NOT NULL,
                    CONSTRAINT [PK_Portfolio] PRIMARY KEY ([Id])
                );");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[SavingsAccount]') IS NULL
                CREATE TABLE [SavingsAccount] (
                    [Id] int NOT NULL IDENTITY,
                    [UserId] int NOT NULL,
                    [SavingsType] nvarchar(50) NOT NULL,
                    [Balance] decimal(18,2) NOT NULL,
                    [AccruedInterest] decimal(18,2) NOT NULL,
                    [AnnualPercentageYield] decimal(6,4) NOT NULL,
                    [MaturityDate] datetime2 NULL,
                    [AccountStatus] nvarchar(20) NOT NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [UpdatedAt] datetime2 NULL,
                    [AccountName] nvarchar(100) NULL,
                    [FundingAccountId] int NULL,
                    [TargetAmount] decimal(18,2) NULL,
                    [TargetDate] datetime2 NULL,
                    CONSTRAINT [PK_SavingsAccount] PRIMARY KEY ([Id])
                );");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[TransactionCategoryOverrides]') IS NULL
                CREATE TABLE [TransactionCategoryOverrides] (
                    [Id] int NOT NULL IDENTITY,
                    [TransactionId] int NOT NULL,
                    [UserId] int NOT NULL,
                    [CategoryId] int NOT NULL,
                    CONSTRAINT [PK_TransactionCategoryOverrides] PRIMARY KEY ([Id])
                );");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[UserCardPreferences]') IS NULL
                CREATE TABLE [UserCardPreferences] (
                    [Id] int NOT NULL,
                    [SortOption] nvarchar(100) NOT NULL,
                    [UpdatedAt] datetime2 NOT NULL,
                    CONSTRAINT [PK_UserCardPreferences] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_UserCardPreferences_Users_Id] FOREIGN KEY ([Id]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
                );");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[ChatMessage]') IS NULL
                CREATE TABLE [ChatMessage] (
                    [Id] int NOT NULL IDENTITY,
                    [ChatSessionId] int NOT NULL,
                    [Sender] nvarchar(20) NOT NULL,
                    [Content] nvarchar(4000) NOT NULL,
                    [SentAt] datetime2 NOT NULL,
                    CONSTRAINT [PK_ChatMessage] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_ChatMessage_ChatSession_ChatSessionId] FOREIGN KEY ([ChatSessionId]) REFERENCES [ChatSession] ([Id]) ON DELETE CASCADE
                );");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[AmortizationRow]') IS NULL
                CREATE TABLE [AmortizationRow] (
                    [Id] int NOT NULL IDENTITY,
                    [LoanId] int NOT NULL,
                    [InstallmentNumber] int NOT NULL,
                    [DueDate] datetime2 NOT NULL,
                    [PrincipalPortion] decimal(18,2) NOT NULL,
                    [InterestPortion] decimal(18,2) NOT NULL,
                    [RemainingBalance] decimal(18,2) NOT NULL,
                    [IsCurrent] bit NOT NULL,
                    CONSTRAINT [PK_AmortizationRow] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_AmortizationRow_Loan_LoanId] FOREIGN KEY ([LoanId]) REFERENCES [Loan] ([Id]) ON DELETE CASCADE
                );");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[InvestmentHolding]') IS NULL
                CREATE TABLE [InvestmentHolding] (
                    [Id] int NOT NULL IDENTITY,
                    [PortfolioId] int NOT NULL,
                    [Ticker] nvarchar(20) NOT NULL,
                    [AssetType] nvarchar(50) NOT NULL,
                    [Quantity] decimal(18,8) NOT NULL,
                    [AvgPurchasePrice] decimal(18,4) NOT NULL,
                    [CurrentPrice] decimal(18,4) NOT NULL,
                    [UnrealizedGainLoss] decimal(18,4) NOT NULL,
                    CONSTRAINT [PK_InvestmentHolding] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_InvestmentHolding_Portfolio_PortfolioId] FOREIGN KEY ([PortfolioId]) REFERENCES [Portfolio] ([Id]) ON DELETE CASCADE
                );");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[AutoDeposit]') IS NULL
                CREATE TABLE [AutoDeposit] (
                    [Id] int NOT NULL IDENTITY,
                    [SavingsAccountId] int NOT NULL,
                    [Amount] decimal(18,2) NOT NULL,
                    [Frequency] nvarchar(20) NOT NULL,
                    [NextRunDate] datetime2 NOT NULL,
                    [IsActive] bit NOT NULL,
                    [SourceAccountId] int NULL,
                    [DayOfMonth] int NULL,
                    [DayOfWeek] int NULL,
                    [UpdatedAt] datetime2 NULL,
                    CONSTRAINT [PK_AutoDeposit] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_AutoDeposit_SavingsAccount_SavingsAccountId] FOREIGN KEY ([SavingsAccountId]) REFERENCES [SavingsAccount] ([Id]) ON DELETE CASCADE
                );");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[SavingsTransaction]') IS NULL
                CREATE TABLE [SavingsTransaction] (
                    [Id] int NOT NULL IDENTITY,
                    [SavingsAccountId] int NOT NULL,
                    [Amount] decimal(18,2) NOT NULL,
                    [Type] nvarchar(20) NOT NULL,
                    [Source] nvarchar(100) NULL,
                    [AccountId] int NOT NULL,
                    [BalanceAfter] decimal(18,2) NOT NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [Description] nvarchar(500) NULL,
                    CONSTRAINT [PK_SavingsTransaction] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_SavingsTransaction_SavingsAccount_SavingsAccountId] FOREIGN KEY ([SavingsAccountId]) REFERENCES [SavingsAccount] ([Id]) ON DELETE CASCADE
                );");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[ChatAttachment]') IS NULL
                CREATE TABLE [ChatAttachment] (
                    [id] int NOT NULL IDENTITY,
                    [messageId] int NOT NULL,
                    [attachmentName] nvarchar(255) NOT NULL,
                    [fileType] nvarchar(50) NOT NULL,
                    [fileSizeBytes] bigint NOT NULL,
                    [storageUrl] nvarchar(255) NOT NULL,
                    CONSTRAINT [PK_ChatAttachment] PRIMARY KEY ([id]),
                    CONSTRAINT [FK_ChatAttachment_ChatMessage_messageId] FOREIGN KEY ([messageId]) REFERENCES [ChatMessage] ([Id]) ON DELETE CASCADE
                );");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[InvestmentTransaction]') IS NULL
                CREATE TABLE [InvestmentTransaction] (
                    [Id] int NOT NULL IDENTITY,
                    [HoldingId] int NOT NULL,
                    [Ticker] nvarchar(20) NOT NULL,
                    [ActionType] nvarchar(10) NOT NULL,
                    [Quantity] decimal(18,8) NOT NULL,
                    [PricePerUnit] decimal(18,4) NOT NULL,
                    [Fees] decimal(18,4) NOT NULL,
                    [OrderType] nvarchar(50) NOT NULL,
                    [ExecutedAt] datetime2 NOT NULL,
                    [InvestmentHoldingId] int NULL,
                    CONSTRAINT [PK_InvestmentTransaction] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_InvestmentTransaction_InvestmentHolding_InvestmentHoldingId] FOREIGN KEY ([InvestmentHoldingId]) REFERENCES [InvestmentHolding] ([Id])
                );");

            // Idempotent index creations — skip if the index already exists
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AmortizationRow_LoanId' AND object_id = OBJECT_ID(N'[AmortizationRow]'))
                CREATE INDEX [IX_AmortizationRow_LoanId] ON [AmortizationRow] ([LoanId]);");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AutoDeposit_SavingsAccountId' AND object_id = OBJECT_ID(N'[AutoDeposit]'))
                CREATE INDEX [IX_AutoDeposit_SavingsAccountId] ON [AutoDeposit] ([SavingsAccountId]);");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChatAttachment_messageId' AND object_id = OBJECT_ID(N'[ChatAttachment]'))
                CREATE INDEX [IX_ChatAttachment_messageId] ON [ChatAttachment] ([messageId]);");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChatMessage_ChatSessionId' AND object_id = OBJECT_ID(N'[ChatMessage]'))
                CREATE INDEX [IX_ChatMessage_ChatSessionId] ON [ChatMessage] ([ChatSessionId]);");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChatSession_UserId' AND object_id = OBJECT_ID(N'[ChatSession]'))
                CREATE INDEX [IX_ChatSession_UserId] ON [ChatSession] ([UserId]);");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_InvestmentHolding_PortfolioId_Ticker' AND object_id = OBJECT_ID(N'[InvestmentHolding]'))
                CREATE UNIQUE INDEX [IX_InvestmentHolding_PortfolioId_Ticker] ON [InvestmentHolding] ([PortfolioId], [Ticker]);");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_InvestmentTransaction_HoldingId' AND object_id = OBJECT_ID(N'[InvestmentTransaction]'))
                CREATE INDEX [IX_InvestmentTransaction_HoldingId] ON [InvestmentTransaction] ([HoldingId]);");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_InvestmentTransaction_InvestmentHoldingId' AND object_id = OBJECT_ID(N'[InvestmentTransaction]'))
                CREATE INDEX [IX_InvestmentTransaction_InvestmentHoldingId] ON [InvestmentTransaction] ([InvestmentHoldingId]);");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Loan_UserId' AND object_id = OBJECT_ID(N'[Loan]'))
                CREATE INDEX [IX_Loan_UserId] ON [Loan] ([UserId]);");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LoanApplication_UserId' AND object_id = OBJECT_ID(N'[LoanApplication]'))
                CREATE INDEX [IX_LoanApplication_UserId] ON [LoanApplication] ([UserId]);");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OAuthLinks_Provider_ProviderUserId' AND object_id = OBJECT_ID(N'[OAuthLinks]'))
                CREATE UNIQUE INDEX [IX_OAuthLinks_Provider_ProviderUserId] ON [OAuthLinks] ([Provider], [ProviderUserId]);");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OAuthLinks_UserId' AND object_id = OBJECT_ID(N'[OAuthLinks]'))
                CREATE INDEX [IX_OAuthLinks_UserId] ON [OAuthLinks] ([UserId]);");

            migrationBuilder.Sql(@"IF (NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PasswordResetTokens_TokenHash' AND object_id = OBJECT_ID(N'[PasswordResetTokens]'))
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[PasswordResetTokens]') AND name = 'TokenHash'))
                CREATE INDEX [IX_PasswordResetTokens_TokenHash] ON [PasswordResetTokens] ([TokenHash]);");

            migrationBuilder.Sql(@"IF (NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PasswordResetTokens_UserId' AND object_id = OBJECT_ID(N'[PasswordResetTokens]'))
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[PasswordResetTokens]') AND name = 'UserId'))
                CREATE INDEX [IX_PasswordResetTokens_UserId] ON [PasswordResetTokens] ([UserId]);");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Portfolio_UserId' AND object_id = OBJECT_ID(N'[Portfolio]'))
                CREATE UNIQUE INDEX [IX_Portfolio_UserId] ON [Portfolio] ([UserId]);");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SavingsAccount_UserId' AND object_id = OBJECT_ID(N'[SavingsAccount]'))
                CREATE INDEX [IX_SavingsAccount_UserId] ON [SavingsAccount] ([UserId]);");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SavingsTransaction_SavingsAccountId' AND object_id = OBJECT_ID(N'[SavingsTransaction]'))
                CREATE INDEX [IX_SavingsTransaction_SavingsAccountId] ON [SavingsTransaction] ([SavingsAccountId]);");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TransactionCategoryOverrides_CategoryId' AND object_id = OBJECT_ID(N'[TransactionCategoryOverrides]'))
                CREATE INDEX [IX_TransactionCategoryOverrides_CategoryId] ON [TransactionCategoryOverrides] ([CategoryId]);");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TransactionCategoryOverrides_TransactionId' AND object_id = OBJECT_ID(N'[TransactionCategoryOverrides]'))
                CREATE INDEX [IX_TransactionCategoryOverrides_TransactionId] ON [TransactionCategoryOverrides] ([TransactionId]);");

            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TransactionCategoryOverrides_UserId' AND object_id = OBJECT_ID(N'[TransactionCategoryOverrides]'))
                CREATE INDEX [IX_TransactionCategoryOverrides_UserId] ON [TransactionCategoryOverrides] ([UserId]);");
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
