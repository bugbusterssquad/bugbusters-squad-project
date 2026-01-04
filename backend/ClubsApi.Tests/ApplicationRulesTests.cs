using ClubsApi.Models;
using ClubsApi.Services;

namespace ClubsApi.Tests;

public class ApplicationRulesTests
{
    [Fact]
    public void ValidateReapply_ReturnsActiveError_WhenPending()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var error = ApplicationRules.ValidateReapply(
            MembershipStatus.Pending,
            MembershipStatus.Pending,
            MembershipStatus.Approved,
            MembershipStatus.Rejected,
            null,
            now,
            30,
            "Zaten aktif bir başvurunuz var.",
            "Reddedilen başvurudan sonra 30 gün beklemelisiniz.");

        Assert.Equal("Zaten aktif bir başvurunuz var.", error);
    }

    [Fact]
    public void ValidateReapply_ReturnsActiveError_WhenApproved()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var error = ApplicationRules.ValidateReapply(
            MembershipStatus.Approved,
            MembershipStatus.Pending,
            MembershipStatus.Approved,
            MembershipStatus.Rejected,
            now.AddDays(-1),
            now,
            30,
            "Zaten aktif bir başvurunuz var.",
            "Reddedilen başvurudan sonra 30 gün beklemelisiniz.");

        Assert.Equal("Zaten aktif bir başvurunuz var.", error);
    }

    [Fact]
    public void ValidateReapply_ReturnsCooldownError_WhenRejectedRecently()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var error = ApplicationRules.ValidateReapply(
            MembershipStatus.Rejected,
            MembershipStatus.Pending,
            MembershipStatus.Approved,
            MembershipStatus.Rejected,
            now.AddDays(-5),
            now,
            30,
            "Zaten aktif bir başvurunuz var.",
            "Reddedilen başvurudan sonra 30 gün beklemelisiniz.");

        Assert.Equal("Reddedilen başvurudan sonra 30 gün beklemelisiniz.", error);
    }

    [Fact]
    public void ValidateReapply_ReturnsNull_WhenRejectedAfterCooldown()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var error = ApplicationRules.ValidateReapply(
            MembershipStatus.Rejected,
            MembershipStatus.Pending,
            MembershipStatus.Approved,
            MembershipStatus.Rejected,
            now.AddDays(-45),
            now,
            30,
            "Zaten aktif bir başvurunuz var.",
            "Reddedilen başvurudan sonra 30 gün beklemelisiniz.");

        Assert.Null(error);
    }
}
