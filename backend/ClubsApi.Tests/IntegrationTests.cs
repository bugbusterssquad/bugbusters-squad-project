using System.Net.Http.Headers;
using System.Net.Http.Json;
using ClubsApi.Data;
using ClubsApi.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ClubsApi.Tests;

public class IntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public IntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Auth_Login_And_Me_ReturnsUser()
    {
        using var client = _factory.CreateClient();
        var token = await LoginAsync(client, "student@bugbusters.dev", "Student123!");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var me = await client.GetFromJsonAsync<AuthUser>("api/auth/me");

        Assert.NotNull(me);
        Assert.Equal("student@bugbusters.dev", me!.Email);
        Assert.Equal("Student", me.Role);
    }

    [Fact]
    public async Task Student_Browse_Clubs_Logs_View()
    {
        using var client = _factory.CreateClient();
        var clubs = await client.GetFromJsonAsync<List<ClubListItem>>("api/clubs");

        Assert.NotNull(clubs);
        Assert.NotEmpty(clubs!);

        var clubId = clubs![0].Id;
        var club = await client.GetFromJsonAsync<ClubDetailItem>($"api/clubs/{clubId}");

        Assert.NotNull(club);
        Assert.Equal(clubId, club!.Id);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasView = db.AnalyticsEvents.Any(e => e.EventName == "club_viewed" && e.EntityId == clubId);

        Assert.True(hasView);
    }

    [Fact]
    public async Task ClubAdmin_Create_And_Publish_Event()
    {
        using var client = await CreateAuthorizedClientAsync("admin1@bugbusters.dev", "ClubAdmin123!");

        var startAt = DateTime.UtcNow.AddDays(5);
        var endAt = startAt.AddHours(2);
        var createRes = await client.PostAsJsonAsync("api/clubs/1/events", new
        {
            title = "Integration Test Event",
            description = "Test event",
            location = "Test Hall",
            startAt,
            endAt,
            capacity = 25
        });
        createRes.EnsureSuccessStatusCode();

        var created = await createRes.Content.ReadFromJsonAsync<CreateEventResponse>();
        Assert.NotNull(created);

        var updateRes = await client.PatchAsJsonAsync($"api/events/{created!.Id}", new
        {
            id = created.Id,
            clubId = 1,
            title = "Integration Test Event",
            description = "Test event",
            location = "Test Hall",
            startAt,
            endAt,
            capacity = 25,
            status = "Published"
        });
        updateRes.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var ev = db.Events.FirstOrDefault(e => e.Id == created.Id);
        Assert.NotNull(ev);
        Assert.Equal(EventStatus.Published, ev!.Status);

        var hasAudit = db.AuditLogs.Any(a => a.EntityType == "event" && a.EntityId == created.Id);
        Assert.True(hasAudit);
    }

    [Fact]
    public async Task Sks_Review_Club_Application()
    {
        using var adminClient = await CreateAuthorizedClientAsync("admin1@bugbusters.dev", "ClubAdmin123!");
        var submitRes = await adminClient.PostAsJsonAsync("api/sks/club-applications", new { clubId = 1 });
        submitRes.EnsureSuccessStatusCode();

        int applicationId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            applicationId = db.SksClubApplications.OrderByDescending(a => a.CreatedAt).Select(a => a.Id).First();
        }

        using var sksClient = await CreateAuthorizedClientAsync("sks@bugbusters.dev", "SksAdmin123!");
        var reviewRes = await sksClient.PatchAsJsonAsync($"api/sks/club-applications/{applicationId}", new
        {
            status = "Approved",
            reviewNote = "Integration test approval"
        });
        reviewRes.EnsureSuccessStatusCode();

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var updated = verifyDb.SksClubApplications.FirstOrDefault(a => a.Id == applicationId);
        Assert.NotNull(updated);
        Assert.Equal(SksApplicationStatus.Approved, updated!.Status);

        var hasAudit = verifyDb.AuditLogs.Any(a => a.EntityType == "sks_club_application" && a.EntityId == applicationId);
        Assert.True(hasAudit);
    }

    private async Task<string> LoginAsync(HttpClient client, string email, string password)
    {
        var res = await client.PostAsJsonAsync("api/auth/login", new { email, password });
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<LoginResponse>();
        return body?.Token ?? string.Empty;
    }

    private async Task<HttpClient> CreateAuthorizedClientAsync(string email, string password)
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, email, password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public record AuthUser(int Id, string Name, string Email, string Role);
    public record LoginResponse(string Token, AuthUser User);
    public record ClubListItem(int Id, string Name, string? Description, string? Category, string? LogoUrl);
    public record ClubDetailItem(int Id, string Name, string? Description, string? Category, string? Contact, string? LogoUrl, string Status, DateTime CreatedAt);
    public record CreateEventResponse(string Message, int Id);
}
