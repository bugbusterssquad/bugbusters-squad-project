using ClubsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ClubsApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Membership> Memberships => Set<Membership>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Club>(e =>
        {
            e.ToTable("ogrenci_kulupleri");
            e.HasKey(x => x.Id);

            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Mission);
            e.Property(x => x.Management);
            e.Property(x => x.Contact);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("kullanici");
            e.HasKey(x => x.Id);

            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.Email).IsRequired();
            e.Property(x => x.Password).IsRequired();
        });

        modelBuilder.Entity<Membership>(e =>
        {
            e.ToTable("kulup_uyelikleri");
            e.HasKey(x => x.Id);

            e.Property(x => x.Status).HasDefaultValue("pending");

            e.HasOne(x => x.User)
             .WithMany(u => u.Memberships)
             .HasForeignKey(x => x.UserId);

            e.HasOne(x => x.Club)
             .WithMany()
             .HasForeignKey(x => x.ClubId);
        });
    }
}
