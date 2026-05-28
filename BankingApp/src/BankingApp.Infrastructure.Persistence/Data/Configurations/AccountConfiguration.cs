namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.Aggregates.AccountAggregate;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(account => account.Id);
        builder.Ignore(account => account.DomainEvents);

        builder.Property(account => account.UserId).IsRequired();
        builder.Property(account => account.AccountName).HasMaxLength(100);
        builder.Property(account => account.Iban)
            .HasConversion(PersistenceValueConverters.IbanConverter)
            .HasMaxLength(34)
            .IsRequired();
        builder.HasIndex(account => account.Iban).IsUnique();
        builder.Property(account => account.Balance)
            .HasConversion(PersistenceValueConverters.MoneyConverter)
            .Metadata.SetValueComparer(PersistenceValueConverters.MoneyComparer);
        builder.Property(account => account.Balance).HasMaxLength(64).IsRequired();
        builder.Property(account => account.AccountType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(account => account.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .HasDefaultValue(AccountStatus.Active)
            .IsRequired();
        builder.Property(account => account.CreatedAt).IsRequired();

        builder.Navigation(account => account.Cards).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(account => account.Transactions).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(account => account.Cards, cards =>
        {
            cards.ToTable("Cards");
            cards.WithOwner().HasForeignKey(card => card.AccountId);
            cards.HasKey(card => card.Id);
            cards.Property(card => card.Id).ValueGeneratedOnAdd();
            cards.Property(card => card.UserId).IsRequired();
            cards.Property(card => card.CardNumber).HasMaxLength(32).IsRequired();
            cards.Property(card => card.CardholderName).HasMaxLength(200).IsRequired();
            cards.Property(card => card.ExpiryDate).IsRequired();
            cards.Property(card => card.Cvv).HasMaxLength(4).IsRequired();
            cards.Property(card => card.CardType).HasConversion<string>().HasMaxLength(32).IsRequired();
            cards.Property(card => card.CardBrand).HasMaxLength(32);
            cards.Property(card => card.Status).HasConversion<string>().HasMaxLength(32).HasDefaultValue(CardStatus.Active);
            cards.Property(card => card.DailyTransactionLimit).HasColumnType("decimal(18,2)");
            cards.Property(card => card.MonthlySpendingCap).HasColumnType("decimal(18,2)");
            cards.Property(card => card.AtmWithdrawalLimit).HasColumnType("decimal(18,2)");
            cards.Property(card => card.ContactlessLimit).HasColumnType("decimal(18,2)");
            cards.Property(card => card.IsContactlessEnabled).HasDefaultValue(true);
            cards.Property(card => card.IsOnlineEnabled).HasDefaultValue(true);
            cards.Property(card => card.SortOrder).HasDefaultValue(0);
            cards.Property(card => card.CancelledAt);
            cards.Property(card => card.CreatedAt).IsRequired();
        });

        builder.OwnsMany(account => account.Transactions, transactions =>
        {
            transactions.ToTable("Transactions");
            transactions.WithOwner().HasForeignKey(transaction => transaction.AccountId);
            transactions.HasKey(transaction => transaction.Id);
            transactions.Property(transaction => transaction.Id).ValueGeneratedOnAdd();
            transactions.Property(transaction => transaction.CardId);
            transactions.Property(transaction => transaction.TransactionRef).HasMaxLength(64).IsRequired();
            transactions.HasIndex(transaction => transaction.TransactionRef).IsUnique();
            transactions.Property(transaction => transaction.Type).HasMaxLength(64).IsRequired();
            transactions.Property(transaction => transaction.Direction).HasConversion<string>().HasMaxLength(16).IsRequired();
            transactions.Property(transaction => transaction.Amount)
                .HasConversion(PersistenceValueConverters.MoneyConverter)
                .Metadata.SetValueComparer(PersistenceValueConverters.MoneyComparer);
            transactions.Property(transaction => transaction.Amount).HasMaxLength(64).IsRequired();
            transactions.Property(transaction => transaction.BalanceAfter)
                .HasConversion(PersistenceValueConverters.MoneyConverter)
                .Metadata.SetValueComparer(PersistenceValueConverters.MoneyComparer);
            transactions.Property(transaction => transaction.BalanceAfter).HasMaxLength(64).IsRequired();
            transactions.Property(transaction => transaction.CounterpartyName).HasMaxLength(200);
            transactions.Property(transaction => transaction.CounterpartyIban).HasMaxLength(34);
            transactions.Property(transaction => transaction.MerchantName).HasMaxLength(200);
            transactions.Property(transaction => transaction.CategoryId);
            transactions.Property(transaction => transaction.Description).HasMaxLength(256);
            transactions.Property(transaction => transaction.Fee)
                .HasConversion(PersistenceValueConverters.NullableMoneyConverter)
                .Metadata.SetValueComparer(PersistenceValueConverters.NullableMoneyComparer);
            transactions.Property(transaction => transaction.Fee).HasMaxLength(64);
            transactions.Property(transaction => transaction.ExchangeRate).HasColumnType("decimal(18,6)");
            transactions.Property(transaction => transaction.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            transactions.Property(transaction => transaction.RelatedEntityType).HasMaxLength(64);
            transactions.Property(transaction => transaction.RelatedEntityId);
            transactions.Property(transaction => transaction.CreatedAt).IsRequired();
        });
    }
}
