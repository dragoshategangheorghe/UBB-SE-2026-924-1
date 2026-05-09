using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Server.DataAccess
{
    public class SessionDAO : ISessionDAO
    {
        private readonly AppDbContext _dbContext;

        public SessionDAO(AppDbContext db)
        {
            this._dbContext = db;
        }

        public Session Create(int userId, string token, string? deviceInfo, string? browser, string? ip)
        {
            var user = _dbContext.Users.Local.FirstOrDefault(u => u.Id == userId) ?? _dbContext.Users.Find(userId) ?? new User { Id = userId };
            if (_dbContext.Entry(user).State == EntityState.Detached)
            {
                _dbContext.Attach(user);
            }

            var session = new Session
            {
                User = user,
                Token = token,
                DeviceInfo = deviceInfo,
                Browser = browser,
                IpAddress = ip,
                LastActiveAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _dbContext.Sessions.Add(session);
            _dbContext.SaveChanges();

            return session;
        }

        public Session? FindByToken(string token)
        {
            return _dbContext.Sessions
                .Include(s => s.User)
                .FirstOrDefault(s => s.Token == token && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow);
        }

        public List<Session> FindByUserId(int userId)
        {
            return _dbContext.Sessions
                .Include(s => s.User)
                .Where(s => s.User.Id == userId && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow)
                .ToList();
        }

        public void Revoke(int sessionId)
        {
            _dbContext.Sessions.Where(s => s.Id == sessionId).ExecuteUpdate(s => s.SetProperty(sess => sess.IsRevoked, true));
        }

        public void RevokeAll(int userId)
        {
            _dbContext.Sessions.Where(s => s.User.Id == userId && !s.IsRevoked).ExecuteUpdate(s => s.SetProperty(sess => sess.IsRevoked, true));
        }
    }
}