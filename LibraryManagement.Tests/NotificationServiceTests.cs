using AwesomeAssertions;
using LibraryManagement.API;
using LibraryManagement.API.Mail;
using LibraryManagement.API.Models.Domain;
using LibraryManagement.API.Repositories;
using LibraryManagement.API.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace LibraryManagement.Tests;

public class NotificationServiceTests {
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ILogger<FeeUpdateService>> _feeManagerLoggerMock = new();
    private readonly Mock<ILogger<NotificationService>> _notificationServiceLoggerMock = new();
    private readonly Mock<IOptions<SmtpSettings>> _smtpSettingsMock = new();
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock = new();
    private readonly Mock<IServiceScope> _scopeMock = new();
    private readonly Mock<IServiceProvider> _serviceProviderMock = new();
    
    private FeeUpdateService _feeUpdateService;
    private NotificationService _classUnderTest;
    private User _testUser;
    
    
    [SetUp]
    public void Setup() {
        _testUser = new User {
            FirstName = "FirstName",
            Name = "Name",
            Email = "Email",
            AccessionDate = DateTime.Now,
        };
        
        _feeUpdateService = new FeeUpdateService(_feeManagerLoggerMock.Object);
        _userRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync([_testUser]);
        
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IUserRepository)))
            .Returns(_userRepoMock.Object);
        _scopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
        _scopeFactoryMock.Setup(f => f.CreateScope()).Returns(_scopeMock.Object);
        
        _classUnderTest = new NotificationService(_feeUpdateService, _notificationServiceLoggerMock.Object, _smtpSettingsMock.Object, _scopeFactoryMock.Object);
    }
    
    #region CheckAnnualFees

    [Test]
    public async Task Given_AccessionDateIsLessThanOneYearAgo_When_RunFeeCalculationAsyncIsCalled_Then_NoAnnualFeeIsAdded() {
        _testUser.AccessionDate = DateTime.Now.AddMonths(-6);

        await _classUnderTest.RunNotificationCheckAsync();

        _testUser.Fees.Should().NotContain(fee => fee.Type == FeeType.Annual);
    }

    [Test]
    public async Task Given_AccessionDateIsMoreThanOneYearAgoAndNoAnnualFeeExists_When_RunFeeCalculationAsyncIsCalled_Then_AnnualFeeIsAdded() {
        _testUser.AccessionDate = DateTime.Now.AddYears(-2);

        await _classUnderTest.RunNotificationCheckAsync();

        _testUser.Fees.Should().ContainSingle(fee => fee.Type == FeeType.Annual);
    }

    [Test]
    public async Task Given_AnnualFeeAlreadyAddedForCurrentYear_When_RunFeeCalculationAsyncIsCalled_Then_NoDuplicateAnnualFeeIsAdded() {
        _testUser.AccessionDate = DateTime.Now.AddYears(-2);
        Fee existingAnnualFee = Fee.CreateAnnualFee(DateTime.Now.Year);
        _testUser.Fees.Add(existingAnnualFee);

        await _classUnderTest.RunNotificationCheckAsync();

        _testUser.Fees.Count(fee => fee.Type == FeeType.Annual).Should().Be(1);
    }

    [Test]
    public async Task Given_LastAnnualFeeWasMoreThanOneYearAgo_When_RunFeeCalculationAsyncIsCalled_Then_NewAnnualFeeIsAdded() {
        _testUser.Loans.Clear();
        Fee oldAnnualFee = Fee.CreateAnnualFee(DateTime.Now.Year - 2);
        oldAnnualFee.CreatedAt = DateTime.Now.AddYears(-2);
        _testUser.Fees.Add(oldAnnualFee);

        await _classUnderTest.RunNotificationCheckAsync();

        _testUser.Fees.Count(fee => fee.Type == FeeType.Annual).Should().Be(2);
    }

    [Test]
    public async Task Given_LastAnnualFeeWasLessThanOneYearAgo_When_RunFeeCalculationAsyncIsCalled_Then_NoNewAnnualFeeIsAdded() {
        _testUser.Loans.Clear();
        Fee recentAnnualFee = Fee.CreateAnnualFee(DateTime.Now.Year);
        recentAnnualFee.CreatedAt = DateTime.Now.AddMonths(-6);
        _testUser.Fees.Add(recentAnnualFee);

        await _classUnderTest.RunNotificationCheckAsync();

        _testUser.Fees.Count(fee => fee.Type == FeeType.Annual).Should().Be(1);
    }

    #endregion
    
    [TearDown]
    public void TearDown() {
        _classUnderTest.Dispose();
    }
}