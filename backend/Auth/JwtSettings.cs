namespace ClubsApi.Auth;

public class JwtSettings
{
    public string Issuer { get; init; } = "BugBusters";
    public string Audience { get; init; } = "BugBusters";
    public string Secret { get; init; } = "dev-secret-change-me-please-1234567890";
    public int ExpMinutes { get; init; } = 120;

    public static JwtSettings FromEnvironment()
    {
        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "BugBusters";
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "BugBusters";
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "dev-secret-change-me-please-1234567890";
        var expRaw = Environment.GetEnvironmentVariable("JWT_EXP_MINUTES");

        var expMinutes = 120;
        if (!string.IsNullOrWhiteSpace(expRaw) && int.TryParse(expRaw, out var parsed))
        {
            expMinutes = parsed;
        }

        return new JwtSettings
        {
            Issuer = issuer,
            Audience = audience,
            Secret = secret,
            ExpMinutes = expMinutes
        };
    }
}
