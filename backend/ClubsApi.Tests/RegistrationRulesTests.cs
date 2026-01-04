using ClubsApi.Models;
using ClubsApi.Services;

namespace ClubsApi.Tests;

public class RegistrationRulesTests
{
    [Fact]
    public void DetermineStatus_ReturnsRegistered_WhenCapacityUnlimited()
    {
        var status = RegistrationRules.DetermineStatus(0, 100);

        Assert.Equal(RegistrationStatus.Registered, status);
    }

    [Fact]
    public void DetermineStatus_ReturnsWaitlist_WhenCapacityFull()
    {
        var status = RegistrationRules.DetermineStatus(5, 5);

        Assert.Equal(RegistrationStatus.Waitlist, status);
    }

    [Fact]
    public void DetermineStatus_ReturnsRegistered_WhenCapacityAvailable()
    {
        var status = RegistrationRules.DetermineStatus(5, 3);

        Assert.Equal(RegistrationStatus.Registered, status);
    }

    [Theory]
    [InlineData(10, 8, 2)]
    [InlineData(10, 10, 0)]
    [InlineData(0, 10, 0)]
    [InlineData(5, 7, 0)]
    public void GetAvailableSlots_ReturnsExpected(int capacity, int registeredCount, int expected)
    {
        var available = RegistrationRules.GetAvailableSlots(capacity, registeredCount);

        Assert.Equal(expected, available);
    }
}
