using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;

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
            var sql = @"INSERT INTO [Session] (UserId, Token, DeviceInfo, Browser, IpAddress, LastActiveAt, ExpiresAt)
                        OUTPUT INSERTED.Id, INSERTED.UserId, INSERTED.Token, INSERTED.DeviceInfo,
                               INSERTED.Browser, INSERTED.IpAddress, INSERTED.LastActiveAt,
                               INSERTED.ExpiresAt, INSERTED.IsRevoked, INSERTED.CreatedAt
                        VALUES (@p0, @p1, @p2, @p3, @p4, GETUTCDATE(), DATEADD(DAY, 7, GETUTCDATE()))";

            using var reader = db.ExecuteQuery(sql, new object[]
            {
                userId,
                token,
                deviceInfo ?? (object)DBNull.Value,
                browser ?? (object)DBNull.Value,
                ip ?? (object)DBNull.Value
            });

            reader.Read();
            return MapSession(reader);
        }

        public Session? FindByToken(string token)
        {
            var sql = @"SELECT Id, UserId, Token, DeviceInfo, Browser, IpAddress,
                        LastActiveAt, ExpiresAt, IsRevoked, CreatedAt
                        FROM [Session] WHERE Token = @p0 AND IsRevoked = 0 AND ExpiresAt > GETUTCDATE()";

            using var reader = db.ExecuteQuery(sql, new object[] { token });
            if (reader.Read())
            {
                return MapSession(reader);
            }
            return null;
        }

        public List<Session> FindByUserId(int userId)
        {
            var sql = @"SELECT Id, UserId, Token, DeviceInfo, Browser, IpAddress,
                        LastActiveAt, ExpiresAt, IsRevoked, CreatedAt
                        FROM [Session] WHERE UserId = @p0 AND IsRevoked = 0 AND ExpiresAt > GETUTCDATE()";

            using var reader = db.ExecuteQuery(sql, new object[] { userId });
            var sessions = new List<Session>();
            while (reader.Read())
            {
                sessions.Add(MapSession(reader));
            }
            return sessions;
        }

        public void Revoke(int sessionId)
        {
            var sql = "UPDATE [Session] SET IsRevoked = 1 WHERE Id = @p0";
            db.ExecuteNonQuery(sql, new object[] { sessionId });
        }

        public void RevokeAll(int userId)
        {
            var sql = "UPDATE [Session] SET IsRevoked = 1 WHERE UserId = @p0 AND IsRevoked = 0";
            db.ExecuteNonQuery(sql, new object[] { userId });
        }

        private Session MapSession(System.Data.IDataReader r)
        {
            return new Session
            {
                Id = r.GetInt32(r.GetOrdinal("Id")),
                UserId = r.GetInt32(r.GetOrdinal("UserId")),
                Token = r.GetString(r.GetOrdinal("Token")),
                DeviceInfo = r.IsDBNull(r.GetOrdinal("DeviceInfo")) ? null : r.GetString(r.GetOrdinal("DeviceInfo")),
                Browser = r.IsDBNull(r.GetOrdinal("Browser")) ? null : r.GetString(r.GetOrdinal("Browser")),
                IpAddress = r.IsDBNull(r.GetOrdinal("IpAddress")) ? null : r.GetString(r.GetOrdinal("IpAddress")),
                LastActiveAt = r.IsDBNull(r.GetOrdinal("LastActiveAt")) ? null : r.GetDateTime(r.GetOrdinal("LastActiveAt")),
                ExpiresAt = r.GetDateTime(r.GetOrdinal("ExpiresAt")),
                IsRevoked = r.GetBoolean(r.GetOrdinal("IsRevoked")),
                CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt"))
            };
        }
    }
}