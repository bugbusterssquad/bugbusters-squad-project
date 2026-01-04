using ClubsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ClubsApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<User> Users => Set<User>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<ClubAdmin> ClubAdmins => Set<ClubAdmin>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();
    public DbSet<ClubMembershipApplication> ClubMembershipApplications => Set<ClubMembershipApplication>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AnalyticsEvent> AnalyticsEvents => Set<AnalyticsEvent>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<SksClubApplication> SksClubApplications => Set<SksClubApplication>();
    public DbSet<SksEventDocument> SksEventDocuments => Set<SksEventDocument>();
    public DbSet<EventComment> EventComments => Set<EventComment>();
    public DbSet<EventReaction> EventReactions => Set<EventReaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.PasswordHash).IsRequired();
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Property(x => x.CreatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.LastLoginAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<StudentProfile>(e =>
        {
            e.ToTable("student_profiles");
            e.HasKey(x => x.UserId);
            e.Property(x => x.Faculty).HasMaxLength(150);
            e.Property(x => x.Department).HasMaxLength(150);
            e.Property(x => x.Bio).HasMaxLength(500);
            e.Property(x => x.AvatarUrl).HasMaxLength(500);
            e.HasOne(x => x.User)
                .WithOne(u => u.StudentProfile)
                .HasForeignKey<StudentProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Club>(e =>
        {
            e.ToTable("clubs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.Category).HasMaxLength(100);
            e.Property(x => x.Contact).HasMaxLength(255);
            e.Property(x => x.LogoUrl).HasMaxLength(500);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue(ClubStatus.Active);
            e.Property(x => x.CreatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<ClubAdmin>(e =>
        {
            e.ToTable("club_admins");
            e.HasKey(x => x.Id);
            e.Property(x => x.RoleInClub).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.CreatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.HasIndex(x => new { x.ClubId, x.UserId }).IsUnique();
            e.HasOne(x => x.User)
                .WithMany(u => u.ClubAdminRoles)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Club)
                .WithMany(c => c.ClubAdmins)
                .HasForeignKey(x => x.ClubId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Event>(e =>
        {
            e.ToTable("events");
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(2000);
            e.Property(x => x.Location).HasMaxLength(200);
            e.Property(x => x.Capacity).HasDefaultValue(0);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue(EventStatus.Draft);
            e.Property(x => x.CreatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.HasOne(x => x.Club)
                .WithMany(c => c.Events)
                .HasForeignKey(x => x.ClubId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EventRegistration>(e =>
        {
            e.ToTable("event_registrations");
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue(RegistrationStatus.Registered);
            e.Property(x => x.CreatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.HasIndex(x => new { x.EventId, x.UserId }).IsUnique();
            e.HasOne(x => x.Event)
                .WithMany(e => e.Registrations)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User)
                .WithMany(u => u.EventRegistrations)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ClubMembershipApplication>(e =>
        {
            e.ToTable("club_membership_applications");
            e.HasKey(x => x.Id);
            e.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(MembershipStatus.Pending);
            e.Property(x => x.Note).HasMaxLength(500);
            e.Property(x => x.CreatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.ReviewedAt).HasColumnType("datetime");
            e.HasIndex(x => new { x.ClubId, x.UserId }).IsUnique();
            e.HasOne(x => x.User)
                .WithMany(u => u.ClubMembershipApplications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Club)
                .WithMany(c => c.ClubMembershipApplications)
                .HasForeignKey(x => x.ClubId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.ToTable("audit_logs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Action).HasMaxLength(100).IsRequired();
            e.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
            e.Property(x => x.MetaJson).HasColumnType("json");
            e.Property(x => x.CreatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.HasOne(x => x.ActorUser)
                .WithMany()
                .HasForeignKey(x => x.ActorUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AnalyticsEvent>(e =>
        {
            e.ToTable("analytics_events");
            e.HasKey(x => x.Id);
            e.Property(x => x.EventName).HasMaxLength(100).IsRequired();
            e.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
            e.Property(x => x.AnonId).HasMaxLength(128);
            e.Property(x => x.MetaJson).HasColumnType("json");
            e.Property(x => x.CreatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.HasIndex(x => new { x.EventName, x.EntityType, x.EntityId });
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Announcement>(e =>
        {
            e.ToTable("announcements");
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Content).HasMaxLength(4000).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue(AnnouncementStatus.Published);
            e.Property(x => x.CreatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.UpdatedAt).HasColumnType("datetime");
            e.HasOne(x => x.Club)
                .WithMany()
                .HasForeignKey(x => x.ClubId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.ToTable("notifications");
            e.HasKey(x => x.Id);
            e.Property(x => x.Type).HasMaxLength(50).IsRequired();
            e.Property(x => x.PayloadJson).HasColumnType("json").IsRequired();
            e.Property(x => x.CreatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.ReadAt).HasColumnType("datetime");
            e.HasIndex(x => new { x.UserId, x.ReadAt });
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SksClubApplication>(e =>
        {
            e.ToTable("sks_club_applications");
            e.HasKey(x => x.Id);
            e.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(SksApplicationStatus.Pending);
            e.Property(x => x.ReviewNote).HasMaxLength(1000);
            e.Property(x => x.CreatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.ReviewedAt).HasColumnType("datetime");
            e.HasIndex(x => x.ClubId).IsUnique();
            e.HasOne(x => x.Club)
                .WithMany()
                .HasForeignKey(x => x.ClubId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.SubmittedByUser)
                .WithMany()
                .HasForeignKey(x => x.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SksEventDocument>(e =>
        {
            e.ToTable("sks_event_documents");
            e.HasKey(x => x.Id);
            e.Property(x => x.FilePath).HasMaxLength(500).IsRequired();
            e.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
            e.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
            e.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(DocumentStatus.Pending);
            e.Property(x => x.ReviewNote).HasMaxLength(1000);
            e.Property(x => x.CreatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.ReviewedAt).HasColumnType("datetime");
            e.HasIndex(x => new { x.EventId, x.Status });
            e.HasOne(x => x.Event)
                .WithMany()
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Club)
                .WithMany()
                .HasForeignKey(x => x.ClubId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.UploadedByUser)
                .WithMany()
                .HasForeignKey(x => x.UploadedByUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EventComment>(e =>
        {
            e.ToTable("event_comments");
            e.HasKey(x => x.Id);
            e.Property(x => x.Body).HasMaxLength(1000).IsRequired();
            e.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(CommentStatus.Visible);
            e.Property(x => x.CreatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.UpdatedAt).HasColumnType("datetime");
            e.HasIndex(x => new { x.EventId, x.Status });
            e.HasOne(x => x.Event)
                .WithMany()
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EventReaction>(e =>
        {
            e.ToTable("event_reactions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Type)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(ReactionType.Like);
            e.Property(x => x.CreatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.HasIndex(x => new { x.EventId, x.UserId, x.Type }).IsUnique();
            e.HasOne(x => x.Event)
                .WithMany()
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        SeedData.Apply(modelBuilder);
    }
}
