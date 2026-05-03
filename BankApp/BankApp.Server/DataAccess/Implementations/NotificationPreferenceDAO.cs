using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankApp.Models.Entities;
using BankApp.Models.Enums;
using BankApp.Models.Extensions;
using BankApp.Server.DataAccess.Interfaces;
namespace BankApp.Server.DataAccess
{
     internal class NotificationPreferenceDAO : INotificationPreferenceDAO
     {
        private AppDbContext appDbContext;

        public NotificationPreferenceDAO(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        public bool Create(int userId, string category)
        {
            try
            {
                string insertQuery = @"INSERT INTO NotificationPreference (UserId, Category, PushEnabled, EmailEnabled, SmsEnabled)
                                        VALUES
                                        (@p0, @p1, 0, 0, 0);
                                    ";

                int rows = this.appDbContext.ExecuteNonQuery(insertQuery, ([userId, category]));

                return rows > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public List<NotificationPreference> FindByUserId(int userId)
        {
            List<NotificationPreference> result = new List<NotificationPreference>();
            string selectQuery = @"SELECT * FROM NotificationPreference WHERE userId = @p0";

            using IDataReader data = this.appDbContext.ExecuteQuery(selectQuery, ([userId]));
            while (data.Read())
            {
                NotificationPreference notificationPreference = new NotificationPreference
                {
                    Id = Convert.ToInt32(data["Id"]),
                    UserId = Convert.ToInt32(data["UserId"]),
                    Category = NotificationTypeExtensions.FromString(Convert.ToString(data["Category"])),
                    PushEnabled = Convert.ToBoolean(data["PushEnabled"]),
                    EmailEnabled = Convert.ToBoolean(data["EmailEnabled"]),
                    SmsEnabled = Convert.ToBoolean(data["SmsEnabled"]),
                    MinAmountThreshold = data["MinAmountThreshold"] == DBNull.Value ? null : Convert.ToDecimal(data["MinAmountThreshold"])
                };

                result.Add(notificationPreference);
            }
            return result;
        }
        public bool Update(int userId, List<NotificationPreference> prefs)
        {
            try
            {
                string deleteQuery = @"DELETE FROM NotificationPreference WHERE userId = @p0";
                this.appDbContext.ExecuteNonQuery(deleteQuery, ([userId]));

                string insertQuery = @"INSERT INTO NotificationPreference (UserId, Category, PushEnabled, EmailEnabled, SmsEnabled, MinAmountThreshold)
                                        VALUES
                                    (@p0, @p1, @p2, @p3, @p4, @p5);
                                ";

                foreach (NotificationPreference preference in prefs)
                {
                    this.appDbContext.ExecuteNonQuery(insertQuery, ([
                            preference.UserId,
                        NotificationTypeExtensions.ToDisplayName(preference.Category),
                        preference.PushEnabled,
                        preference.EmailEnabled,
                        preference.SmsEnabled,
                        preference.MinAmountThreshold!
                        ]));
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
