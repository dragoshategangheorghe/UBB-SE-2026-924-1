using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Server.DataAccess.Implementations
{
    public class UserDAO : IUserDAO
    {
        private readonly AppDbContext _dbContext;

        public UserDAO(AppDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public User? FindByEmail(string email)
        {
            User? user = _dbContext.Users.FirstOrDefault(user => user.Email == email);
            return user;
        }

        public User? FindById(int id)
        {
            User? user = _dbContext.Users.FirstOrDefault(user => user.Id == id);
            return user;
        }

        public bool Create(User user)
        {
            _dbContext.Users.Add(user);
            return _dbContext.SaveChanges() > 0;
        }

        public bool Update(User updatedUser)
        {
            var rows = _dbContext.Users
                .Where(user => user.Id == updatedUser.Id)
                .ExecuteUpdate(setters => setters
                    .SetProperty(user => user.Email, updatedUser.Email)
                    .SetProperty(user => user.FullName, updatedUser.FullName)
                    .SetProperty(user => user.PhoneNumber, updatedUser.PhoneNumber)
                    .SetProperty(user => user.DateOfBirth, updatedUser.DateOfBirth)
                    .SetProperty(user => user.Address, updatedUser.Address)
                    .SetProperty(user => user.Nationality, updatedUser.Nationality)
                    .SetProperty(user => user.PreferredLanguage, updatedUser.PreferredLanguage)
                    .SetProperty(user => user.Is2FAEnabled, updatedUser.Is2FAEnabled)
                    .SetProperty(user => user.Preferred2FAMethod, updatedUser.Preferred2FAMethod)
                    .SetProperty(user => user.UpdatedAt, DateTime.UtcNow));
            return rows > 0;
        }

        public bool UpdatePassword(int userId, string newPasswordHash)
        {
            var rows = _dbContext.Users
                .Where(user => user.Id == userId)
                .ExecuteUpdate(setters => setters
                    .SetProperty(user => user.PasswordHash, newPasswordHash)
                    .SetProperty(user => user.UpdatedAt, DateTime.UtcNow));
            return rows > 0;
        }

        public void IncrementFailedAttempts(int userId)
        {
            _dbContext.Users
                .Where(user => user.Id == userId)
                .ExecuteUpdate(setters => setters
                    .SetProperty(user => user.FailedLoginAttempts, user => user.FailedLoginAttempts + 1));
        }

        public void ResetFailedAttempts(int userId)
        {
            _dbContext.Users
                .Where(user => user.Id == userId)
                .ExecuteUpdate(setters => setters
                    .SetProperty(user => user.FailedLoginAttempts, 0));
        }

        public void LockAccount(int userId, DateTime lockoutEnd)
        {
            _dbContext.Users
                .Where(user => user.Id == userId)
                .ExecuteUpdate(setters => setters
                    .SetProperty(user => user.IsLocked, true)
                    .SetProperty(user => user.LockoutEnd, lockoutEnd));
        }
    }
}
