using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankApp.Server.Services.Implementations;
using NUnit.Framework;
using NSubstitute;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Models.Entities;
using BankApp.Models.DTOs.Dashboard;

namespace BankApp.Server.Tests
{
    [TestFixture]
    public class DashboardServiceTests
    {
        private IDashboardRepository mockDashboardRepository;
        private IUserRepository mockUserRepository;
        private DashboardService dashboardService;

        [SetUp]
        public void SetUp()
        {
            mockDashboardRepository = Substitute.For<IDashboardRepository>();
            mockUserRepository = Substitute.For<IUserRepository>();

            dashboardService = new DashboardService(mockDashboardRepository, mockUserRepository);
        }

        [Test]
        public void GetDashboardData_NoUserWithID_ReturnsNull()
        {
            int userId = 1;
            mockUserRepository.FindById(userId).Returns((User)null!);

            DashboardResponse response = dashboardService.GetDashboardData(userId);

            Assert.That(response, Is.Null);
        }

        [Test]
        public void GetDashboardData_UserWithID_ReturnsFullDashboardResponse()
        {
            int testUserId = 1;
            int testNotificationCount = 3;
            User testUser = new User { Id = testUserId };

            mockUserRepository.FindById(testUserId).Returns(testUser);
            mockDashboardRepository.GetCardsByUser(testUserId).Returns(new List<Card>());
            mockDashboardRepository.GetRecentTransactions(testUserId).Returns(new List<Transaction>());
            mockDashboardRepository.GetUnreadNotificationCount(testUserId).Returns(testNotificationCount);

            DashboardResponse testResponse = dashboardService.GetDashboardData(testUserId);
            DashboardResponse responseToCompareTo = new DashboardResponse
            {
                CurrentUser = testUser,
                Cards = new (),
                RecentTransactions = new (),
                UnreadNotificationCount = testNotificationCount
            };

            Assert.That(testResponse, Is.EqualTo(responseToCompareTo));
        }
    }
}
