using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Server.DataAccess
{
    public class SessionDAO : ISessionDAO
    {
        private readonly AppDbContext db;

        public SessionDAO(AppDbContext db)
        {
            this.db = db;
        }

        public Session Create(int userId, string token, string? deviceInfo, string? browser, string? ip)
        {
            var user = db.Users.Local.FirstOrDefault(u => u.Id == userId) ?? db.Users.Find(userId) ?? new User { Id = userId };
            if (db.Entry(user).State == EntityState.Detached)
            {
                db.Attach(user);
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

            db.Sessions.Add(session);
            db.SaveChanges();

            return session;
        }

        public Session? FindByToken(string token)
        {
            return db.Sessions
                .Include(s => s.User)
                .FirstOrDefault(s => s.Token == token && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow);
        }

        public List<Session> FindByUserId(int userId)
        {
            return db.Sessions
                .Include(s => s.User)
                .Where(s => s.User.Id == userId && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow)
                .ToList();
        }

        public void Revoke(int sessionId)
        {
            db.Sessions.Where(s => s.Id == sessionId).ExecuteUpdate(s => s.SetProperty(sess => sess.IsRevoked, true));
        }

        public void RevokeAll(int userId)
        {
            db.Sessions.Where(s => s.User.Id == userId && !s.IsRevoked).ExecuteUpdate(s => s.SetProperty(sess => sess.IsRevoked, true));
        }
    }
}