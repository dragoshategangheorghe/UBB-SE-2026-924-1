//using BankApp.Models.Entities;
//using Microsoft.EntityFrameworkCore;

//namespace BankApp.Server.DataAccess
//{
//    public class AppDbContext : DbContext
//    {
//        public AppDbContext() { }

//        public AppDbContext(DbContextOptions<AppDbContext> _options) : base(_options) { }

//        // Entity sets for the tables

//        public DbSet<Account> Accounts { get; set; }

//        public DbSet<Card> Cards { get; set; }

//        public DbSet<Category> Categories { get; set; }

//        public DbSet<Notification> Notifications { get; set; }

//        public DbSet<NotificationPreference> NotificationPreferences { get; set; }

//        public DbSet<OAuthLink> OAuthLinks { get; set; }

//        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

//        public DbSet<Session> Sessions { get; set; }

//        public DbSet<Transaction> Transactions { get; set; }

//        public DbSet<TransactionCategoryOverride> TransactionCategoriesOverride { get; set; }

//        public DbSet<User> Users { get; set; }

//        public DbSet<UserCardPreference> UserCardPreferences { get; set; }


//    }
//}
