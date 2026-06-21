using AwesomeAssertions;
using LibraryManagement.API.Models.Domain;
using LibraryManagement.API.Repositories;
using LibraryManagement.API.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagement.Tests;

public class FeeCalculationServiceTests {
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ILoanRepository> _loanRepoMock = new();
    private readonly Mock<ILogger<FeeCalculationService>> _loggerMock = new();
    
    private FeeCalculationService _classUnderTest;
    
    private User _userWithOverdueLoan;
    private Loan _overdueLoan;
    
    
    [SetUp]
    public void Setup() {
        _overdueLoan = new Loan {
            User = _userWithOverdueLoan,
            DueAt = DateTime.Now.AddDays(-2)
        };
        
        _userWithOverdueLoan = new User {
            FirstName = "FirstName",
            Name = "Name",
            Email = "Email",
            AccessionDate = DateTime.Now,
            Loans = [_overdueLoan]
        };
        
        _userRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync([_userWithOverdueLoan]);
        _classUnderTest = new FeeCalculationService(_loggerMock.Object, _userRepoMock.Object, _loanRepoMock.Object);
    }

    
    #region CheckOverdueLoans
    
    [Test]
    public async Task Given_UserHasNoLoans_When_RunFeeCalculationAsyncIsCalled_Then_NoFeeIsAdded() {
        _userWithOverdueLoan.Loans.Clear();
        await _classUnderTest.RunFeeCalculationAsync();
        _userWithOverdueLoan.Fees.Should().BeEmpty();
    }
    
    [Test]
    public async Task Given_LoanIsNotOverdue_When_RunFeeCalculationAsyncIsCalled_Then_NoFeeIsAdded() {
        _overdueLoan.DueAt = DateTime.Now.AddDays(1);
        await _classUnderTest.RunFeeCalculationAsync();
        _userWithOverdueLoan.Fees.Should().BeEmpty();
    }
    
    [Test]
    public async Task Given_UserHasOneDayOverdueLoan_When_RunFeeCalculationAsyncIsCalled_Then_FeeIsAdded() {
        _overdueLoan.DueAt = DateTime.Now.AddDays(-1);
        await _classUnderTest.RunFeeCalculationAsync();
        _userWithOverdueLoan.Fees[0].Amount.Should().Be(0.5m);
    }
    
    [Test]
    public async Task Given_UserHasNineDaysOverdueLoan_When_RunFeeCalculationAsyncIsCalled_Then_FeeIsAdded() {
        _overdueLoan.DueAt = DateTime.Now.AddDays(-9);
        await _classUnderTest.RunFeeCalculationAsync();
        _userWithOverdueLoan.Fees[0].Amount.Should().Be(0.5m);
    }
    
    [Test]
    public async Task Given_LoanIsOverdueMoreThan10Days_When_RunFeeCalculationAsyncIsCalled_Then_DailyFeeIs1Euro() {
        _overdueLoan.DueAt = DateTime.Now.AddDays(-11);
        await _classUnderTest.RunFeeCalculationAsync();
        _userWithOverdueLoan.Fees[0].Amount.Should().Be(1.0m);
    }
    
    [Test]
    public async Task Given_LoanFeeAmountIsNearCap_When_RunFeeCalculationAsyncIsCalled_Then_FeeIsCappedAt15Euro() {
        Fee existingFee = new() { Amount = 14.75m };
        _overdueLoan.Fees.Add(existingFee);
        await _classUnderTest.RunFeeCalculationAsync();
        _overdueLoan.Fees[1].Amount.Should().Be(0.25m);
    }
    
    [Test]
    public async Task Given_LoanFeeAmountIsAtCap_When_RunFeeCalculationAsyncIsCalled_Then_NoFeeIsAdded() {
        Fee existingFee = new() { Amount = 15m };
        _overdueLoan.Fees.Add(existingFee);
        await _classUnderTest.RunFeeCalculationAsync();
        _userWithOverdueLoan.Fees.Count.Should().Be(0);
    }
    
    [Test]
    public async Task Given_TimesRemindedIs0_When_RunFeeCalculationAsyncIsCalled_Then_NoReminderFeeIsAdded() {
        _overdueLoan.TimesReminded = 0;
        await _classUnderTest.RunFeeCalculationAsync();
        _userWithOverdueLoan.Fees.Should().NotContain(fee => fee.Type == FeeType.FirstReminder || fee.Type == FeeType.SecondReminder);
    }

    [Test]
    public async Task Given_TimesRemindedIs1_When_RunFeeCalculationAsyncIsCalled_Then_FirstReminderFeeIsAdded() {
        
        _overdueLoan.TimesReminded = 1;
        await _classUnderTest.RunFeeCalculationAsync();
        _userWithOverdueLoan.Fees.Should().ContainSingle(fee => fee.Type == FeeType.FirstReminder && fee.Amount == 1m);
    }

    [Test]
    public async Task Given_TimesRemindedIs1_And_FirstReminderFeeAlreadyExists_When_RunFeeCalculationAsyncIsCalled_Then_NoDuplicateFirstReminderFeeIsAdded() {
        _overdueLoan.TimesReminded = 1;
        Fee existingFirstReminder = Fee.CreateFirstReminderFee(_overdueLoan.Id);
        _overdueLoan.Fees.Add(existingFirstReminder);
        _userWithOverdueLoan.Fees.Add(existingFirstReminder);
        await _classUnderTest.RunFeeCalculationAsync();
        _userWithOverdueLoan.Fees.Count(fee => fee.Type == FeeType.FirstReminder).Should().Be(1);
    }

    [Test]
    public async Task Given_TimesRemindedIs2_When_RunFeeCalculationAsyncIsCalled_Then_SecondReminderFeeIsAdded() {
        _overdueLoan.TimesReminded = 2;
        await _classUnderTest.RunFeeCalculationAsync();
        _userWithOverdueLoan.Fees.Should().ContainSingle(fee => fee.Type == FeeType.SecondReminder && fee.Amount == 3m);
    }

    [Test]
    public async Task Given_TimesRemindedIs2_And_SecondReminderFeeAlreadyExists_When_RunFeeCalculationAsyncIsCalled_Then_NoDuplicateSecondReminderFeeIsAdded() {
        _overdueLoan.TimesReminded = 2;
        Fee existingSecondReminder = Fee.CreateSecondReminderFee(_overdueLoan.Id);
        _overdueLoan.Fees.Add(existingSecondReminder);
        _userWithOverdueLoan.Fees.Add(existingSecondReminder);
        
        await _classUnderTest.RunFeeCalculationAsync();
        
        _userWithOverdueLoan.Fees.Count(fee => fee.Type == FeeType.SecondReminder).Should().Be(1);
    }
    
    #endregion

    
    
    [TearDown]
    public void TearDown() {
        _classUnderTest.Dispose();
    }
    
    
}