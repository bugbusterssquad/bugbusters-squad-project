namespace ClubsApi.Services;

public static class CommentModerationService
{
    private static readonly string[] BlockedTerms =
    [
        "spam",
        "kufur",
        "hakaret"
    ];

    public static bool ContainsBlockedContent(string input)
    {
        foreach (var term in BlockedTerms)
        {
            if (input.Contains(term, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
