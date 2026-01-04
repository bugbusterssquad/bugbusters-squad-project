using ClubsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ClubsApi.Data;

public static class SeedData
{
    private const string SeedSalt = "$2a$11$abcdefghijklmnopqrstuv";

    public static void Apply(ModelBuilder modelBuilder)
    {
        var seedTime = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        var studentUser = new User
        {
            Id = 1,
            Name = "Ayse Demir",
            Email = "student@bugbusters.dev",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student123!", SeedSalt),
            Role = UserRole.Student,
            CreatedAt = seedTime
        };

        var clubAdminOne = new User
        {
            Id = 2,
            Name = "Ali Yilmaz",
            Email = "admin1@bugbusters.dev",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("ClubAdmin123!", SeedSalt),
            Role = UserRole.ClubAdmin,
            CreatedAt = seedTime
        };

        var clubAdminTwo = new User
        {
            Id = 3,
            Name = "Ece Koc",
            Email = "admin2@bugbusters.dev",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("ClubAdmin123!", SeedSalt),
            Role = UserRole.ClubAdmin,
            CreatedAt = seedTime
        };

        var sksAdmin = new User
        {
            Id = 4,
            Name = "Sks Yetkilisi",
            Email = "sks@bugbusters.dev",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("SksAdmin123!", SeedSalt),
            Role = UserRole.SksAdmin,
            CreatedAt = seedTime
        };

        var superAdmin = new User
        {
            Id = 5,
            Name = "Ops Admin",
            Email = "ops@bugbusters.dev",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("SuperAdmin123!", SeedSalt),
            Role = UserRole.SuperAdmin,
            CreatedAt = seedTime
        };

        modelBuilder.Entity<User>().HasData(studentUser, clubAdminOne, clubAdminTwo, sksAdmin, superAdmin);

        modelBuilder.Entity<StudentProfile>().HasData(new StudentProfile
        {
            UserId = studentUser.Id,
            Faculty = "Muhendislik",
            Department = "Bilgisayar Muhendisligi",
            Bio = "Yapay zeka ve topluluk etkinlikleriyle ilgileniyor.",
            AvatarUrl = null
        });

        modelBuilder.Entity<Club>().HasData(
            new Club
            {
                Id = 1,
                Name = "Yapay Zeka Kulubu",
                Description = "AI projeleri ve arastirmalar.",
                Category = "Teknoloji",
                Contact = "ai@okul.com",
                LogoUrl = null,
                Status = ClubStatus.Active,
                CreatedAt = seedTime
            },
            new Club
            {
                Id = 2,
                Name = "Siber Guvenlik Kulubu",
                Description = "CTF ve siber savunma egitimleri.",
                Category = "Teknoloji",
                Contact = "security@okul.com",
                LogoUrl = null,
                Status = ClubStatus.Active,
                CreatedAt = seedTime
            },
            new Club
            {
                Id = 3,
                Name = "Girisimcilik Kulubu",
                Description = "Startup ve networking etkinlikleri.",
                Category = "Is ve Girisim",
                Contact = "giris@okul.com",
                LogoUrl = null,
                Status = ClubStatus.Active,
                CreatedAt = seedTime
            }
        );

        modelBuilder.Entity<ClubAdmin>().HasData(
            new ClubAdmin
            {
                Id = 1,
                ClubId = 1,
                UserId = 2,
                RoleInClub = ClubAdminRole.Owner,
                CreatedAt = seedTime
            },
            new ClubAdmin
            {
                Id = 2,
                ClubId = 2,
                UserId = 3,
                RoleInClub = ClubAdminRole.Owner,
                CreatedAt = seedTime
            }
        );

        modelBuilder.Entity<Event>().HasData(
            new Event
            {
                Id = 1,
                ClubId = 1,
                Title = "AI 101 Atolyesi",
                Description = "Yeni baslayanlar icin temel AI workshop.",
                Location = "M1 Konferans Salonu",
                StartAt = new DateTime(2025, 2, 10, 18, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2025, 2, 10, 20, 0, 0, DateTimeKind.Utc),
                Capacity = 60,
                Status = EventStatus.Published,
                CreatedAt = seedTime
            },
            new Event
            {
                Id = 2,
                ClubId = 1,
                Title = "Model Degerlendirme Semineri",
                Description = "ML model performansi ve metrikler.",
                Location = "Lab 3",
                StartAt = new DateTime(2025, 2, 18, 17, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2025, 2, 18, 19, 0, 0, DateTimeKind.Utc),
                Capacity = 40,
                Status = EventStatus.Published,
                CreatedAt = seedTime
            },
            new Event
            {
                Id = 3,
                ClubId = 2,
                Title = "CTF Hazirlik Oturumu",
                Description = "Takim olusturma ve alistirmalar.",
                Location = "Siber Lab",
                StartAt = new DateTime(2025, 2, 12, 18, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2025, 2, 12, 20, 0, 0, DateTimeKind.Utc),
                Capacity = 50,
                Status = EventStatus.Published,
                CreatedAt = seedTime
            },
            new Event
            {
                Id = 4,
                ClubId = 3,
                Title = "Startup Pitch Night",
                Description = "Fikirlerinizi juriye sunun.",
                Location = "Etkinlik Salonu",
                StartAt = new DateTime(2025, 2, 20, 18, 30, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2025, 2, 20, 21, 0, 0, DateTimeKind.Utc),
                Capacity = 120,
                Status = EventStatus.Published,
                CreatedAt = seedTime
            },
            new Event
            {
                Id = 5,
                ClubId = 3,
                Title = "Yatirimci ile Kahve",
                Description = "Yatirimci bulusmasi ve mentorluk.",
                Location = "Kulupler Binasi",
                StartAt = new DateTime(2025, 2, 25, 16, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2025, 2, 25, 17, 30, 0, DateTimeKind.Utc),
                Capacity = 30,
                Status = EventStatus.Published,
                CreatedAt = seedTime
            }
        );

        modelBuilder.Entity<ClubMembershipApplication>().HasData(new ClubMembershipApplication
        {
            Id = 1,
            UserId = 1,
            ClubId = 1,
            Status = MembershipStatus.Approved,
            Note = "Seeded membership",
            QrCode = null,
            CreatedAt = seedTime,
            ReviewedAt = seedTime
        });
    }
}
