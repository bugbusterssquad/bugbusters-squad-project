namespace ClubsApi.Services;

public static class ReviewRules
{
    public static string? ValidateReviewTransition<TStatus>(
        TStatus currentStatus,
        TStatus newStatus,
        TStatus pendingStatus,
        TStatus approvedStatus,
        TStatus rejectedStatus,
        string finalizedMessage)
        where TStatus : struct, Enum
    {
        if (!EqualityComparer<TStatus>.Default.Equals(newStatus, approvedStatus) &&
            !EqualityComparer<TStatus>.Default.Equals(newStatus, rejectedStatus))
            return "Sadece Approved veya Rejected verilebilir.";

        if (!EqualityComparer<TStatus>.Default.Equals(currentStatus, pendingStatus))
            return finalizedMessage;

        return null;
    }
}
