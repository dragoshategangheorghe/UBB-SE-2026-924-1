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
        private IDashboardRepository _mockDashboardRepository;
        private IUserRepository _mockUserRepository;
        private DashboardService _dashboardService;

        [SetUp]
        public void SetUp()
        {
            _mockDashboardRepository = Substitute.For<IDashboardRepository>();
            _mockUserRepository = Substitute.For<IUserRepository>();

            _dashboardService = new DashboardService(_mockDashboardRepository, _mockUserRepository);
        }

        [Test]
        public void GetDashboardData_NoUserWithID_ReturnsNull()
        {
            int userId = 1;
            _mockUserRepository.FindById(userId).Returns((User)null!);

            DashboardResponse response = _dashboardService.GetDashboardData(userId);

            Assert.That(response, Is.Null);
        }

        [Test]
        public void GetDashboardData_UserWithID_ReturnsFullDashboardResponse()
        {
            int testUserId = 1;
            int testNotificationCount = 3;
            User testUser = new User { Id = testUserId };

            _mockUserRepository.FindById(testUserId).Returns(testUser);
            _mockDashboardRepository.GetCardsByUser(testUserId).Returns(new List<Card>());
            _mockDashboardRepository.GetRecentTransactions(testUserId).Returns(new List<Transaction>());
            _mockDashboardRepository.GetUnreadNotificationCount(testUserId).Returns(testNotificationCount);

            DashboardResponse testResponse = _dashboardService.GetDashboardData(testUserId);
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
