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
                entity.HasOne(a => a.User)
                    .WithMany(u => u.Accounts)
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(a => a.Cards)
                    .WithOne(c => c.Account)
                    .IsRequired();

                entity.HasMany(a => a.Transactions)
                    .WithOne(t => t.Account)
                    .IsRequired();
            });

            modelBuilder.Entity<Card>(entity =>
            {
                entity.HasOne(c => c.User)
                    .WithMany(u => u.Cards)
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(c => c.Transactions)
                    .WithOne(t => t.Card);
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasOne(t => t.Account)
                    .WithMany(a => a.Transactions)
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(t => t.Card)
                    .WithMany(c => c.Transactions);

                entity.HasOne(t => t.Category)
                    .WithMany();
            });

            modelBuilder.Entity<Session>(entity =>
            {
                entity.HasOne(s => s.User)
                    .WithMany(u => u.Sessions)
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasOne(n => n.User)
                    .WithMany(u => u.Notifications)
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<OAuthLink>(entity =>
            {
                entity.HasOne(o => o.User)
                    .WithMany(u => u.OAuthLinks)
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<NotificationPreference>(entity =>
            {
                entity.Property(p => p.Category)
                    .HasConversion<string>();

                entity.HasOne(p => p.User)
                    .WithMany(u => u.NotificationPreferences)
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasOne(p => p.User)
                    .WithMany(u => u.PasswordResetTokens)
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<UserCardPreference>(entity =>
            {
                entity.HasKey(ucp => ucp.UserId);

                entity.HasOne(p => p.User)
                    .WithMany(u => u.UserCardPreferences)
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<TransactionCategoryOverride>(entity =>
            {
                entity.HasOne(t => t.Transaction)
                    .WithMany()
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(t => t.User)
                    .WithMany(u => u.TransactionCategoryOverrides)
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(t => t.Category)
                    .WithMany()
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Loan>(entity =>
            {
                entity.HasOne(l => l.User)
                    .WithMany()
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(l => l.AmortizationRows)
                    .WithOne(a => a.Loan)
                    .IsRequired();
            });

            modelBuilder.Entity<SavingsAccount>(entity =>
            {
                entity.HasOne(s => s.User)
                    .WithMany()
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(s => s.FundingAccount)
                    .WithMany().OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(s => s.AutoDeposits)
                    .WithOne(a => a.SavingsAccount)
                    .HasForeignKey(a => a.SavingsAccountId)
                    .IsRequired();

                entity.HasMany(s => s.Transactions)
                    .WithOne(t => t.SavingsAccount)
                    .HasForeignKey(t => t.AccountId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<ChatSession>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.IssueCategory).HasColumnName("issueCategory").HasMaxLength(50);
                entity.Property(e => e.SessionStatus).HasColumnName("sessionStatus").HasMaxLength(30);
                entity.Property(e => e.Rating).HasColumnName("rating");
                entity.Property(e => e.StartedAt).HasColumnName("startedAt");
                entity.Property(e => e.EndedAt).HasColumnName("endedAt");
                entity.Property(e => e.Feedback).HasColumnName("feedback").HasMaxLength(255);
                entity.Property<int>("UserId").HasColumnName("userId");

                entity.HasOne(s => s.User)
                    .WithMany()
                    .HasForeignKey("UserId")
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(s => s.Messages)
                    .WithOne(m => m.Session)
                    .IsRequired();
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.SessionId).HasColumnName("sessionId");
                entity.Property(e => e.SenderType).HasColumnName("senderType").HasMaxLength(20);
                entity.Property(e => e.Content).HasColumnName("content");
                entity.Property(e => e.SentAt).HasColumnName("sentAt");
            });

            modelBuilder.Entity<ChatAttachment>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.MessageId).HasColumnName("messageId");
                entity.Property(e => e.AttachmentName).HasColumnName("attachmentName").HasMaxLength(255);
                entity.Property(e => e.FileType).HasColumnName("fileType").HasMaxLength(50);
                entity.Property(e => e.FileSizeBytes).HasColumnName("fileSizeBytes");
                entity.Property(e => e.StorageUrl).HasColumnName("storageUrl").HasMaxLength(255);

                entity.HasOne(a => a.Message)
                    .WithMany()
                    .HasForeignKey(a => a.MessageId)
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);
            });

            // --- Investment Feature Deep Configuration ---
            modelBuilder.Entity<BankApp.Models.Entities.Portfolio>(entity =>
            {
                entity.HasKey(p => p.IdentificationNumber); // Ensure PK matches Entity

                entity.HasOne(p => p.User)
                    .WithMany()
                    .HasForeignKey(p => p.UserId) // Explicit FK mapping
                    .IsRequired().OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(p => p.Holdings)
                    .WithOne(h => h.Portfolio)
                    .HasForeignKey(h => h.PortfolioId) // Explicit FK mapping
                    .IsRequired();
            });

            modelBuilder.Entity<BankApp.Models.Entities.InvestmentHolding>(entity =>
            {
                entity.HasKey(h => h.IdentificationNumber);

                entity.HasMany(h => h.Transactions)
                    .WithOne(t => t.Holding)
                    .HasForeignKey(t => t.HoldingId) // Explicit FK mapping
                    .IsRequired();
            });

            modelBuilder.Entity<BankApp.Models.Entities.InvestmentTransaction>(entity =>
            {
                entity.HasKey(t => t.IdentificationNumber);
            });
        }
    }
}