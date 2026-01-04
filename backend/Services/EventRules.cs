namespace ClubsApi.Services;

public static class EventRules
{
    public static string? ValidateEvent(DateTime startAt, DateTime endAt, int capacity, DateTime? nowUtc = null)
    {
        var now = nowUtc ?? DateTime.UtcNow;

        if (capacity < 0)
            return "Kapasite negatif olamaz.";

        if (endAt <= startAt)
            return "Bitiş tarihi başlangıçtan sonra olmalıdır.";

        if (startAt < now)
            return "Başlangıç tarihi geçmiş olamaz.";

        return null;
    }
}
