using ClubsApi.Models;

namespace ClubsApi.Services;

public static class RegistrationRules
{
    public static RegistrationStatus DetermineStatus(int capacity, int registeredCount)
    {
        if (capacity <= 0)
            return RegistrationStatus.Registered;

        return registeredCount >= capacity ? RegistrationStatus.Waitlist : RegistrationStatus.Registered;
    }

    public static int GetAvailableSlots(int capacity, int registeredCount)
    {
        if (capacity <= 0)
            return 0;

        var available = capacity - registeredCount;
        return available > 0 ? available : 0;
    }
}
