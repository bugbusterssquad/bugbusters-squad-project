using ClubsApi.Models;
using ClubsApi.Services;

namespace ClubsApi.Tests;

public class ReviewRulesTests
{
    [Fact]
    public void ValidateReviewTransition_ReturnsInvalidMessage_WhenStatusNotAllowed()
    {
        var error = ReviewRules.ValidateReviewTransition(
            MembershipStatus.Pending,
            MembershipStatus.Pending,
            MembershipStatus.Pending,
            MembershipStatus.Approved,
            MembershipStatus.Rejected,
            "Başvuru zaten sonuçlanmış.");

        Assert.Equal("Sadece Approved veya Rejected verilebilir.", error);
    }

    [Fact]
    public void ValidateReviewTransition_ReturnsFinalizedMessage_WhenNotPending()
    {
        var error = ReviewRules.ValidateReviewTransition(
            SksApplicationStatus.Approved,
            SksApplicationStatus.Rejected,
            SksApplicationStatus.Pending,
            SksApplicationStatus.Approved,
            SksApplicationStatus.Rejected,
            "Başvuru zaten sonuçlanmış.");

        Assert.Equal("Başvuru zaten sonuçlanmış.", error);
    }

    [Fact]
    public void ValidateReviewTransition_ReturnsNull_WhenPendingAndApproved()
    {
        var error = ReviewRules.ValidateReviewTransition(
            DocumentStatus.Pending,
            DocumentStatus.Approved,
            DocumentStatus.Pending,
            DocumentStatus.Approved,
            DocumentStatus.Rejected,
            "Belge zaten sonuçlanmış.");

        Assert.Null(error);
    }
}
