using System.Collections.Generic;
using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using BankApp.Server.Repositories.Implementations;
using Moq;
using Xunit;

public class UserRepositoryTests
{
    private readonly Mock<IUserDAO> userDaoMock;
    private readonly Mock<ISessionDAO> sessionDaoMock;
    private readonly Mock<IOAuthLinkDAO> oauthDaoMock;
    private readonly Mock<INotificationPreferenceDAO> notifDaoMock;

    private readonly UserRepository userRepository;

    public UserRepositoryTests()
    {
        userDaoMock = new Mock<IUserDAO>();
        sessionDaoMock = new Mock<ISessionDAO>();
        oauthDaoMock = new Mock<IOAuthLinkDAO>();
        notifDaoMock = new Mock<INotificationPreferenceDAO>();

        userRepository = new UserRepository(
            userDaoMock.Object,
            sessionDaoMock.Object,
            oauthDaoMock.Object,
            notifDaoMock.Object);
    }

    [Fact]
    public void UpdateUser_ReturnsTrue_WhenSuccessful()
    {
        var user = new User { Id = 1 };
        userDaoMock.Setup(d => d.Update(user)).Returns(true);

        var result = userRepository.UpdateUser(user);

        Assert.True(result);
    }

    [Fact]
    public void UpdateUser_ReturnFalse_WhenSuccess()
    {
        var user = new User { Id = 1 };
        userDaoMock.Setup(d => d.Update(user)).Returns(true);

        var result = userRepository.UpdateUser(user);

        Assert.False(result);
    }

    // Session
    [Fact]
    public void GetActiveSessions_ReturnsSessions()
    {
        var sessions = new List<Session> { new Session { Id = 1 } };
        sessionDaoMock.Setup(d => d.FindByUserId(1)).Returns(sessions);

        var result = userRepository.GetActiveSessions(1);

        Assert.Single(result);
    }

    [Fact]
    public void DeleteOAuthLink_CallsDao()
    {
        userRepository.DeleteOAuthLink(10);

        oauthDaoMock.Verify(d => d.Delete(10), Times.Once);
    }

    // Notification Preferences
    [Fact]
    public void GetNotificationPreferences_ReturnsPreferences()
    {
        var prefs = new List<NotificationPreference> { new NotificationPreference { Id = 1 } };

        notifDaoMock.Setup(d => d.FindByUserId(1)).Returns(prefs);

        var result = userRepository.GetNotificationPreferences(1);

        Assert.Single(result);
    }

    [Fact]
    public void UpdateNotificationPreferences_ReturnsTrue()
    {
        var prefs = new List<NotificationPreference> { new NotificationPreference { Id = 1 } };

        notifDaoMock.Setup(d => d.Update(1, prefs)).Returns(true);

        var result = userRepository.UpdateNotificationPreferences(1, prefs);

        Assert.True(result);
    }
}