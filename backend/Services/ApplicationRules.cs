namespace ClubsApi.Services;

public static class ApplicationRules
{
    public static string? ValidateReapply<TStatus>(
        TStatus status,
        TStatus pendingStatus,
        TStatus approvedStatus,
        TStatus rejectedStatus,
        DateTime? reviewedAt,
        DateTime nowUtc,
        int cooldownDays,
        string activeMessage,
        string cooldownMessage)
        where TStatus : struct, Enum
    {
        if (EqualityComparer<TStatus>.Default.Equals(status, pendingStatus) ||
            EqualityComparer<TStatus>.Default.Equals(status, approvedStatus))
            return activeMessage;

        if (EqualityComparer<TStatus>.Default.Equals(status, rejectedStatus) &&
            reviewedAt.HasValue &&
            reviewedAt.Value > nowUtc.AddDays(-cooldownDays))
            return cooldownMessage;

        return null;
    }
}
