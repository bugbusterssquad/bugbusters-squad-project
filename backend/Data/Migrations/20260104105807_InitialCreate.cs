using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ClubsApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "clubs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Contact = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LogoUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Active")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clubs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastLoginAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ClubId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Location = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Draft")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_events_clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ActorUserId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EntityType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    MetaJson = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_logs_users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "club_admins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ClubId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleInClub = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_club_admins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_club_admins_clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_club_admins_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "club_membership_applications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClubId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Note = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QrCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ReviewedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_club_membership_applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_club_membership_applications_clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_club_membership_applications_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "student_profiles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Faculty = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Department = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Bio = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AvatarUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_profiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_student_profiles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "event_registrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Registered")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_registrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_event_registrations_events_EventId",
                        column: x => x.EventId,
                        principalTable: "events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_event_registrations_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "clubs",
                columns: new[] { "Id", "Category", "Contact", "CreatedAt", "Description", "LogoUrl", "Name" },
                values: new object[,]
                {
                    { 1, "Teknoloji", "ai@okul.com", new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "AI projeleri ve arastirmalar.", null, "Yapay Zeka Kulubu" },
                    { 2, "Teknoloji", "security@okul.com", new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "CTF ve siber savunma egitimleri.", null, "Siber Guvenlik Kulubu" },
                    { 3, "Is ve Girisim", "giris@okul.com", new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Startup ve networking etkinlikleri.", null, "Girisimcilik Kulubu" }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "Id", "CreatedAt", "Email", "LastLoginAt", "Name", "PasswordHash", "Role" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "student@bugbusters.dev", null, "Ayse Demir", "$2a$11$abcdefghijklmnopqrstuusvjCjxykWM6EVr4NLmYOZDEUJzz8Mcm", "Student" },
                    { 2, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "admin1@bugbusters.dev", null, "Ali Yilmaz", "$2a$11$abcdefghijklmnopqrstuunexzIc3x8I4UQSfyinzLbbkD0PvhD6a", "ClubAdmin" },
                    { 3, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "admin2@bugbusters.dev", null, "Ece Koc", "$2a$11$abcdefghijklmnopqrstuunexzIc3x8I4UQSfyinzLbbkD0PvhD6a", "ClubAdmin" },
                    { 4, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "sks@bugbusters.dev", null, "Sks Yetkilisi", "$2a$11$abcdefghijklmnopqrstuutK6kgbr7Fw8OIjF7TSn68Opi3gTMwfq", "SksAdmin" },
                    { 5, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "ops@bugbusters.dev", null, "Ops Admin", "$2a$11$abcdefghijklmnopqrstuuUV5XOpUP0aNxeeIpIwHSsKodZk708Ru", "SuperAdmin" }
                });

            migrationBuilder.InsertData(
                table: "club_admins",
                columns: new[] { "Id", "ClubId", "CreatedAt", "RoleInClub", "UserId" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Owner", 2 },
                    { 2, 2, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Owner", 3 }
                });

            migrationBuilder.InsertData(
                table: "club_membership_applications",
                columns: new[] { "Id", "ClubId", "CreatedAt", "Note", "QrCode", "ReviewedAt", "Status", "UserId" },
                values: new object[] { 1, 1, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Seeded membership", null, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Approved", 1 });

            migrationBuilder.InsertData(
                table: "events",
                columns: new[] { "Id", "Capacity", "ClubId", "CreatedAt", "Description", "EndAt", "Location", "StartAt", "Status", "Title" },
                values: new object[,]
                {
                    { 1, 60, 1, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Yeni baslayanlar icin temel AI workshop.", new DateTime(2025, 2, 10, 20, 0, 0, 0, DateTimeKind.Utc), "M1 Konferans Salonu", new DateTime(2025, 2, 10, 18, 0, 0, 0, DateTimeKind.Utc), "Published", "AI 101 Atolyesi" },
                    { 2, 40, 1, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "ML model performansi ve metrikler.", new DateTime(2025, 2, 18, 19, 0, 0, 0, DateTimeKind.Utc), "Lab 3", new DateTime(2025, 2, 18, 17, 0, 0, 0, DateTimeKind.Utc), "Published", "Model Degerlendirme Semineri" },
                    { 3, 50, 2, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Takim olusturma ve alistirmalar.", new DateTime(2025, 2, 12, 20, 0, 0, 0, DateTimeKind.Utc), "Siber Lab", new DateTime(2025, 2, 12, 18, 0, 0, 0, DateTimeKind.Utc), "Published", "CTF Hazirlik Oturumu" },
                    { 4, 120, 3, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Fikirlerinizi juriye sunun.", new DateTime(2025, 2, 20, 21, 0, 0, 0, DateTimeKind.Utc), "Etkinlik Salonu", new DateTime(2025, 2, 20, 18, 30, 0, 0, DateTimeKind.Utc), "Published", "Startup Pitch Night" },
                    { 5, 30, 3, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Yatirimci bulusmasi ve mentorluk.", new DateTime(2025, 2, 25, 17, 30, 0, 0, DateTimeKind.Utc), "Kulupler Binasi", new DateTime(2025, 2, 25, 16, 0, 0, 0, DateTimeKind.Utc), "Published", "Yatirimci ile Kahve" }
                });

            migrationBuilder.InsertData(
                table: "student_profiles",
                columns: new[] { "UserId", "AvatarUrl", "Bio", "Department", "Faculty" },
                values: new object[] { 1, null, "Yapay zeka ve topluluk etkinlikleriyle ilgileniyor.", "Bilgisayar Muhendisligi", "Muhendislik" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_ActorUserId",
                table: "audit_logs",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_club_admins_ClubId_UserId",
                table: "club_admins",
                columns: new[] { "ClubId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_club_admins_UserId",
                table: "club_admins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_club_membership_applications_ClubId_UserId",
                table: "club_membership_applications",
                columns: new[] { "ClubId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_club_membership_applications_UserId",
                table: "club_membership_applications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_clubs_Name",
                table: "clubs",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_event_registrations_EventId_UserId",
                table: "event_registrations",
                columns: new[] { "EventId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_event_registrations_UserId",
                table: "event_registrations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_events_ClubId",
                table: "events",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "club_admins");

            migrationBuilder.DropTable(
                name: "club_membership_applications");

            migrationBuilder.DropTable(
                name: "event_registrations");

            migrationBuilder.DropTable(
                name: "student_profiles");

            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "clubs");
        }
    }
}
