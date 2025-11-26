using ClubsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ClubsApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<User> Users => Set<User>(); // Yeni eklenen satır

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Club>(e =>
        {
            e.ToTable("ogrenci_kulupleri");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
        });

        // Yeni eklenen ayarlar
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("kullanici"); // SQL'deki tablo adı
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).IsRequired();
        });
    }
}
