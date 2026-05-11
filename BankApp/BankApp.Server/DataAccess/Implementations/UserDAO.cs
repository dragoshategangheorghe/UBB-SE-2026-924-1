using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Server.DataAccess.Implementations
{
    public class UserDAO : IUserDAO
    {
        private readonly AppDbContext db;

        public UserDAO(AppDbContext db)
        {
            this.db = db;
        }

        public User? FindByEmail(string email)
        {
            User? user = db.Users.FirstOrDefault(u => u.Email == email);
            return user;
        }

        public User? FindById(int id)
        {
            User? user = db.Users.FirstOrDefault(u => u.Id == id);
            return user;
        }

        public bool Create(User user)
        {
            db.Users.Add(user);
            return db.SaveChanges() > 0;
        }

        public bool Update(User user)
        {
            var rows = db.Users
                .Where(u => u.Id == user.Id)
                .ExecuteUpdate(s => s
                    .SetProperty(u => u.Email, user.Email)
                    .SetProperty(u => u.FullName, user.FullName)
                    .SetProperty(u => u.PhoneNumber, user.PhoneNumber)
                    .SetProperty(u => u.DateOfBirth, user.DateOfBirth)
                    .SetProperty(u => u.Address, user.Address)
                    .SetProperty(u => u.Nationality, user.Nationality)
                    .SetProperty(u => u.PreferredLanguage, user.PreferredLanguage)
                    .SetProperty(u => u.Is2FAEnabled, user.Is2FAEnabled)
                    .SetProperty(u => u.Preferred2FAMethod, user.Preferred2FAMethod)
                    .SetProperty(u => u.UpdatedAt, DateTime.UtcNow));

            return rows > 0;
        }

        public bool UpdatePassword(int userId, string newPasswordHash)
        {
            var rows = db.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdate(s => s
                    .SetProperty(u => u.PasswordHash, newPasswordHash)
                    .SetProperty(u => u.UpdatedAt, DateTime.UtcNow));
            return rows > 0;
        }

        public void IncrementFailedAttempts(int userId)
        {
            db.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdate(s => s
                    .SetProperty(u => u.FailedLoginAttempts, u => u.FailedLoginAttempts + 1));
        }

        public void ResetFailedAttempts(int userId)
        {
            db.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdate(s => s
                    .SetProperty(u => u.FailedLoginAttempts, 0));
        }

        public void LockAccount(int userId, DateTime lockoutEnd)
        {
            db.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdate(s => s
                    .SetProperty(u => u.IsLocked, true)
                    .SetProperty(u => u.LockoutEnd, lockoutEnd));
        }
    }
}
