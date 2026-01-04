using ClubsApi.Services;

namespace ClubsApi.Tests;

public class EventRulesTests
{
    [Fact]
    public void ValidateEvent_ReturnsError_WhenCapacityNegative()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var start = now.AddDays(1);
        var end = start.AddHours(2);

        var error = EventRules.ValidateEvent(start, end, -1, now);

        Assert.Equal("Kapasite negatif olamaz.", error);
    }

    [Fact]
    public void ValidateEvent_ReturnsError_WhenEndBeforeStart()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var start = now.AddDays(1);
        var end = start.AddMinutes(-10);

        var error = EventRules.ValidateEvent(start, end, 10, now);

        Assert.Equal("Bitiş tarihi başlangıçtan sonra olmalıdır.", error);
    }

    [Fact]
    public void ValidateEvent_ReturnsError_WhenStartInPast()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var start = now.AddMinutes(-5);
        var end = now.AddHours(1);

        var error = EventRules.ValidateEvent(start, end, 10, now);

        Assert.Equal("Başlangıç tarihi geçmiş olamaz.", error);
    }

    [Fact]
    public void ValidateEvent_ReturnsNull_WhenValid()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var start = now.AddHours(2);
        var end = now.AddHours(4);

        var error = EventRules.ValidateEvent(start, end, 0, now);

        Assert.Null(error);
    }
}
