using ClubsApi.Auth;
using ClubsApi.Models;
using ClubsApi.Services;

namespace ClubsApi.Tests;

public class JwtTokenServiceTests
{
    [Fact]
    public void CreateToken_ReturnsJwt()
    {
        var settings = new JwtSettings
        {
            Issuer = "BugBusters",
            Audience = "BugBusters",
            Secret = "unit-test-secret-should-be-long-enough",
            ExpMinutes = 60
        };

        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@bugbusters.dev",
            Role = UserRole.Student
        };

        var service = new JwtTokenService(settings);
        var token = service.CreateToken(user);

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Equal(3, token.Split('.').Length);
    }
}
