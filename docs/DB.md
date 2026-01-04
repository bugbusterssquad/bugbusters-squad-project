# Veritabanı Dokümantasyonu

## Genel

- **DB:** MySQL
- **Varsayılan DB adı:** `clubsdb`
- **Charset/Collation:** `utf8mb4` / `utf8mb4_turkish_ci`
- **Migration kaynağı:** `backend/Data/Migrations`
- **Manuel kurulum:** `database_creation.sql`

## Şema Özeti (Ana Tablolar)

- `users`: kullanıcılar, rol ve login bilgileri
- `student_profiles`: öğrenci profili (1:1 users)
- `clubs`: kulüp bilgileri
- `club_admins`: kulüp yöneticileri (clubId-userId eşlemesi)
- `events`: etkinlikler (clubId ile ilişkili)
- `event_registrations`: etkinlik kayıtları (Registered/Cancelled/Waitlist)
- `announcements`: duyurular (clubId ile ilişkili)
- `club_membership_applications`: kulübe katılım başvuruları
- `notifications`: in-app bildirimler (payload JSON)
- `sks_club_applications`: SKS kulüp başvuruları
- `sks_event_documents`: etkinlik belge akışı (dosya yolu + review)
- `event_comments`: sosyal yorumlar (Visible/Hidden/Deleted)
- `event_reactions`: sosyal beğeni (Like)
- `audit_logs`: kritik aksiyon logları
- `analytics_events`: görüntüleme ve DAU logları

## Temel İlişkiler

- `clubs` → `events`, `announcements`, `club_admins`, `club_membership_applications`, `sks_club_applications`
- `events` → `event_registrations`, `sks_event_documents`, `event_comments`, `event_reactions`
- `users` → `student_profiles`, `club_admins`, `event_registrations`, `event_comments`, `event_reactions`, `notifications`

## Status Değerleri

- `UserRole`: `Student`, `ClubAdmin`, `SksAdmin`, `SuperAdmin`
- `ClubStatus`: `Active`, `Passive`
- `EventStatus`: `Draft`, `Published`, `Cancelled`
- `AnnouncementStatus`: `Published`, `Hidden`
- `MembershipStatus`: `Pending`, `Approved`, `Rejected`
- `DocumentStatus`: `Pending`, `Approved`, `Rejected`
- `CommentStatus`: `Visible`, `Hidden`, `Deleted`
- `RegistrationStatus`: `Registered`, `Cancelled`, `Waitlist`
- `ReactionType`: `Like`

## Seed Data

Seed kayıtları EF Core üzerinden `SeedData` ile yüklenir. `database_creation.sql` içinde de aynı seed seti bulunur.

- Öğrenci: `student@bugbusters.dev`
- Kulüp Admin: `admin1@bugbusters.dev`, `admin2@bugbusters.dev`
- SKS Admin: `sks@bugbusters.dev`
- Super Admin: `ops@bugbusters.dev`

## Migration Notları

- Uygulama açılışında `MIGRATE_ON_STARTUP=true` ise `db.Database.Migrate()` çalışır.
- Manuel kurulum gerekiyorsa:
  - `MIGRATE_ON_STARTUP=false`
  - `mysql -u root -p < database_creation.sql`
