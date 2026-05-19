using BankApp.Models.Entities;
using BankApp.Models.Features.Chat;
using BankApp.Models.Features.Investments;
using BankApp.Models.Features.Loans;
using BankApp.Models.Features.Savings;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Server.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // --- Core Tables ---
        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationPreference> NotificationPreferences { get; set; }
        public DbSet<OAuthLink> OAuthLinks { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionCategoryOverride> TransactionCategoriesOverride { get; set; }
        public DbSet<UserCardPreference> UserCardPreferences { get; set; }

        // --- FEATURE: Chat ---
        public DbSet<AttachmentUploadResponse> AttachmentUploadResponses { get; set; }
        public DbSet<ChatAttachment> ChatAttachments { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatSession> ChatSessions { get; set; }

        // --- FEATURE: Investments ---
        public DbSet<FundingSourceOption> FundingSourceOptions { get; set; }
        public DbSet<BankApp.Models.Entities.InvestmentHolding> InvestmentHoldings { get; set; }
        public DbSet<BankApp.Models.Entities.Portfolio> Portfolios { get; set; }
        public DbSet<BankApp.Models.Entities.InvestmentTransaction> InvestmentTransactions { get; set; }
        public DbSet<SelectedAttachment> SelectedAttachments { get; set; }

        // --- FEATURE: Loans ---
        public DbSet<AmortizationRow> AmortizationRows { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<LoanApplication> LoanApplications { get; set; }
        public DbSet<LoanEstimate> LoanEstimates { get; set; }

        // --- FEATURE: Savings ---
        public DbSet<AutoDeposit> AutoDeposits { get; set; }
        public DbSet<SavingsAccount> SavingsAccounts { get; set; }
        public DbSet<SavingsTransaction> SavingsTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var decimalProperty in modelBuilder.Model.GetEntityTypes()
                         .SelectMany(entityType => entityType.GetProperties())
                         .Where(property => property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?)))
            {
                decimalProperty.SetPrecision(18);
                decimalProperty.SetScale(2);
            }

            // Table Name Mappings
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Session>().ToTable("Session");
            modelBuilder.Entity<OAuthLink>().ToTable("OAuthLink");
            modelBuilder.Entity<Account>().ToTable("Account");
            modelBuilder.Entity<Card>().ToTable("Card");
            modelBuilder.Entity<Category>().ToTable("Category");
            modelBuilder.Entity<Transaction>().ToTable("Transaction");
            modelBuilder.Entity<Notification>().ToTable("Notification");
            modelBuilder.Entity<NotificationPreference>().ToTable("NotificationPreference");
            modelBuilder.Entity<PasswordResetToken>().ToTable("PasswordResetToken");
            modelBuilder.Entity<TransactionCategoryOverride>().ToTable("TransactionCategoryOverride");
            modelBuilder.Entity<UserCardPreference>().ToTable("UserCardPreference");
            modelBuilder.Entity<Loan>().ToTable("Loan");
            modelBuilder.Entity<LoanApplication>().ToTable("LoanApplication");
            modelBuilder.Entity<AmortizationRow>().ToTable("AmortizationRow");
            modelBuilder.Entity<SavingsAccount>().ToTable("SavingsAccount");
            modelBuilder.Entity<SavingsTransaction>().ToTable("SavingsTransaction");
            modelBuilder.Entity<AutoDeposit>().ToTable("AutoDeposit");
            modelBuilder.Entity<ChatSession>().ToTable("ChatSession");
            modelBuilder.Entity<ChatMessage>().ToTable("ChatMessage");
            modelBuilder.Entity<ChatAttachment>().ToTable("ChatAttachment");

            // Investment Feature Table Mappings
            modelBuilder.Entity<BankApp.Models.Entities.Portfolio>().ToTable("Portfolio");
            modelBuilder.Entity<BankApp.Models.Entities.InvestmentHolding>().ToTable("InvestmentHolding");
            modelBuilder.Entity<BankApp.Models.Entities.InvestmentTransaction>().ToTable("InvestmentTransaction");

            //--- Relationship Configurations ---
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasOne(account => account.User)
                    .WithMany(user => user.Accounts)
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(account => account.Cards)
                    .WithOne(card => card.Account)
                    .IsRequired();

                entity.HasMany(account => account.Transactions)
                    .WithOne(transaction => transaction.Account)
                    .IsRequired();
            });

            modelBuilder.Entity<Card>(entity =>
            {
                entity.HasOne(card => card.User)
                    .WithMany(user => user.Cards)
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(card => card.Transactions)
                    .WithOne(transaction => transaction.Card);
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasOne(transaction => transaction.Account)
                    .WithMany(account => account.Transactions)
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(transaction => transaction.Card)
                    .WithMany(card => card.Transactions);

                entity.HasOne(transaction => transaction.Category)
                    .WithMany();
            });

            modelBuilder.Entity<Session>(entity =>
            {
                entity.HasOne(session => session.User)
                    .WithMany(user => user.Sessions)
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasOne(notification => notification.User)
                    .WithMany(user => user.Notifications)
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<OAuthLink>(entity =>
            {
                entity.HasOne(oauthLink => oauthLink.User)
                    .WithMany(user => user.OAuthLinks)
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<NotificationPreference>(entity =>
            {
                entity.Property(notificationPreference => notificationPreference.Category)
                    .HasConversion<string>();

                entity.HasOne(notificationPreference => notificationPreference.User)
                    .WithMany(user => user.NotificationPreferences)
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasOne(passwordResetToken => passwordResetToken.User)
                    .WithMany(user => user.PasswordResetTokens)
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<UserCardPreference>(entity =>
            {
                entity.HasKey(userCardPreference => userCardPreference.UserId);

                entity.HasOne(userCardPreference => userCardPreference.User)
                    .WithMany(user => user.UserCardPreferences)
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<TransactionCategoryOverride>(entity =>
            {
                entity.HasOne(transactionCategoryOverride => transactionCategoryOverride.Transaction)
                    .WithMany()
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(transactionCategoryOverride => transactionCategoryOverride.User)
                    .WithMany(user => user.TransactionCategoryOverrides)
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(transactionCategoryOverride => transactionCategoryOverride.Category)
                    .WithMany()
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Loan>(entity =>
            {
                entity.HasOne(loan => loan.User)
                    .WithMany()
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(loan => loan.AmortizationRows)
                    .WithOne(a => a.Loan)
                    .IsRequired();
            });

            modelBuilder.Entity<SavingsAccount>(entity =>
            {
                entity.HasOne(savingsAccount => savingsAccount.User)
                    .WithMany()
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(savingsAccount => savingsAccount.FundingAccount)
                    .WithMany().OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(savingsAccount => savingsAccount.AutoDeposits)
                    .WithOne(autoDeposit => autoDeposit.SavingsAccount)
                    .HasForeignKey(autoDeposit => autoDeposit.SavingsAccountId)
                    .IsRequired();

                entity.HasMany(savingsAccount => savingsAccount.Transactions)
                    .WithOne(transaction => transaction.SavingsAccount)
                    .HasForeignKey(transaction => transaction.AccountId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<ChatSession>(entity =>
            {
                entity.Property(chatSession => chatSession.Id).HasColumnName("id");
                entity.Property(chatSession => chatSession.IssueCategory).HasColumnName("issueCategory").HasMaxLength(50);
                entity.Property(chatSession => chatSession.SessionStatus).HasColumnName("sessionStatus").HasMaxLength(30);
                entity.Property(chatSession => chatSession.Rating).HasColumnName("rating");
                entity.Property(chatSession => chatSession.StartedAt).HasColumnName("startedAt");
                entity.Property(chatSession => chatSession.EndedAt).HasColumnName("endedAt");
                entity.Property(chatSession => chatSession.Feedback).HasColumnName("feedback").HasMaxLength(255);
                entity.Property<int>("UserId").HasColumnName("userId");

                entity.HasOne(chatSession => chatSession.User)
                    .WithMany()
                    .HasForeignKey("UserId")
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(chatSession => chatSession.Messages)
                    .WithOne(m => m.Session)
                    .IsRequired();
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.Property(chatMessage => chatMessage.Id).HasColumnName("id");
                entity.Property(chatMessage => chatMessage.SessionId).HasColumnName("sessionId");
                entity.Property(chatMessage => chatMessage.SenderType).HasColumnName("senderType").HasMaxLength(20);
                entity.Property(chatMessage => chatMessage.Content).HasColumnName("content");
                entity.Property(chatMessage => chatMessage.SentAt).HasColumnName("sentAt");
            });

            modelBuilder.Entity<ChatAttachment>(entity =>
            {
                entity.Property(chatAttachment => chatAttachment.Id).HasColumnName("id");
                entity.Property(chatAttachment => chatAttachment.MessageId).HasColumnName("messageId");
                entity.Property(chatAttachment => chatAttachment.AttachmentName).HasColumnName("attachmentName").HasMaxLength(255);
                entity.Property(chatAttachment => chatAttachment.FileType).HasColumnName("fileType").HasMaxLength(50);
                entity.Property(chatAttachment => chatAttachment.FileSizeBytes).HasColumnName("fileSizeBytes");
                entity.Property(chatAttachment => chatAttachment.StorageUrl).HasColumnName("storageUrl").HasMaxLength(255);

                entity.HasOne(chatAttachment => chatAttachment.Message)
                    .WithMany()
                    .HasForeignKey(chatAttachment => chatAttachment.MessageId)
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);
            });

            // --- Investment Feature Deep Configuration ---
            modelBuilder.Entity<BankApp.Models.Entities.Portfolio>(entity =>
            {
                entity.HasKey(portfolio => portfolio.Id); // Ensure PK matches Entity

                entity.HasOne(portfolio => portfolio.User)
                    .WithMany()
                    .HasForeignKey(portfolio => portfolio.UserId) // Explicit FK mapping
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(portfolio => portfolio.Holdings)
                    .WithOne(holding => holding.Portfolio)
                    .HasForeignKey(holding => holding.PortfolioId) // Explicit FK mapping
                    .IsRequired();
            });

            modelBuilder.Entity<BankApp.Models.Entities.InvestmentHolding>(entity =>
            {
                entity.HasKey(holding => holding.Id);

                entity.HasMany(holding => holding.Transactions)
                    .WithOne(transaction => transaction.Holding)
                    .HasForeignKey(transaction => transaction.HoldingId) // Explicit FK mapping
                    .IsRequired();
            });

            modelBuilder.Entity<BankApp.Models.Entities.InvestmentTransaction>(entity =>
            {
                entity.HasKey(transaction => transaction.Id);
            });
        }
    }
}
