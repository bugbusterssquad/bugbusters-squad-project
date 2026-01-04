CREATE DATABASE IF NOT EXISTS clubsdb
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_turkish_ci;

USE clubsdb;

-- =============================================
-- 1) USERS
-- =============================================
CREATE TABLE IF NOT EXISTS users (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  Name VARCHAR(150) NOT NULL,
  Email VARCHAR(200) NOT NULL UNIQUE,
  PasswordHash TEXT NOT NULL,
  Role VARCHAR(20) NOT NULL,
  CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  LastLoginAt DATETIME NULL
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_turkish_ci;

-- =============================================
-- 2) STUDENT PROFILES
-- =============================================
CREATE TABLE IF NOT EXISTS student_profiles (
  UserId INT PRIMARY KEY,
  Faculty VARCHAR(150) NULL,
  Department VARCHAR(150) NULL,
  Bio VARCHAR(500) NULL,
  AvatarUrl VARCHAR(500) NULL,
  FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_turkish_ci;

-- =============================================
-- 3) CLUBS
-- =============================================
CREATE TABLE IF NOT EXISTS clubs (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  Name VARCHAR(150) NOT NULL UNIQUE,
  Description VARCHAR(1000) NULL,
  Category VARCHAR(100) NULL,
  Contact VARCHAR(255) NULL,
  LogoUrl VARCHAR(500) NULL,
  Status VARCHAR(20) NOT NULL DEFAULT 'Active',
  CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_turkish_ci;

-- =============================================
-- 4) CLUB ADMINS
-- =============================================
CREATE TABLE IF NOT EXISTS club_admins (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  ClubId INT NOT NULL,
  UserId INT NOT NULL,
  RoleInClub VARCHAR(20) NOT NULL,
  CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UNIQUE KEY uq_club_admin (ClubId, UserId),
  FOREIGN KEY (ClubId) REFERENCES clubs(Id) ON DELETE CASCADE,
  FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_turkish_ci;

-- =============================================
-- 5) EVENTS
-- =============================================
CREATE TABLE IF NOT EXISTS events (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  ClubId INT NOT NULL,
  Title VARCHAR(200) NOT NULL,
  Description VARCHAR(2000) NULL,
  Location VARCHAR(200) NULL,
  StartAt DATETIME NOT NULL,
  EndAt DATETIME NOT NULL,
  Capacity INT NOT NULL DEFAULT 0,
  Status VARCHAR(20) NOT NULL DEFAULT 'Draft',
  CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (ClubId) REFERENCES clubs(Id) ON DELETE CASCADE
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_turkish_ci;

-- =============================================
-- 6) ANNOUNCEMENTS
-- =============================================
CREATE TABLE IF NOT EXISTS announcements (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  ClubId INT NOT NULL,
  Title VARCHAR(200) NOT NULL,
  Content VARCHAR(4000) NOT NULL,
  Status VARCHAR(20) NOT NULL DEFAULT 'Published',
  CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UpdatedAt DATETIME NULL,
  FOREIGN KEY (ClubId) REFERENCES clubs(Id) ON DELETE CASCADE
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_turkish_ci;

-- =============================================
-- 7) SKS CLUB APPLICATIONS
-- =============================================
CREATE TABLE IF NOT EXISTS sks_club_applications (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  ClubId INT NOT NULL,
  SubmittedByUserId INT NOT NULL,
  Status VARCHAR(20) NOT NULL DEFAULT 'Pending',
  ReviewNote VARCHAR(1000) NULL,
  CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ReviewedAt DATETIME NULL,
  UNIQUE KEY uq_sks_club (ClubId),
  FOREIGN KEY (ClubId) REFERENCES clubs(Id) ON DELETE CASCADE,
  FOREIGN KEY (SubmittedByUserId) REFERENCES users(Id) ON DELETE CASCADE
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_turkish_ci;

-- =============================================
-- 8) SKS EVENT DOCUMENTS
-- =============================================
CREATE TABLE IF NOT EXISTS sks_event_documents (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  EventId INT NOT NULL,
  ClubId INT NOT NULL,
  UploadedByUserId INT NOT NULL,
  FilePath VARCHAR(500) NOT NULL,
  OriginalFileName VARCHAR(255) NOT NULL,
  ContentType VARCHAR(100) NOT NULL,
  Status VARCHAR(20) NOT NULL DEFAULT 'Pending',
  ReviewNote VARCHAR(1000) NULL,
  CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ReviewedAt DATETIME NULL,
  INDEX idx_event_status (EventId, Status),
  FOREIGN KEY (EventId) REFERENCES events(Id) ON DELETE CASCADE,
  FOREIGN KEY (ClubId) REFERENCES clubs(Id) ON DELETE CASCADE,
  FOREIGN KEY (UploadedByUserId) REFERENCES users(Id) ON DELETE CASCADE
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_turkish_ci;

-- =============================================
-- 9) EVENT COMMENTS
-- =============================================
CREATE TABLE IF NOT EXISTS event_comments (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  EventId INT NOT NULL,
  UserId INT NOT NULL,
  Body VARCHAR(1000) NOT NULL,
  Status VARCHAR(20) NOT NULL DEFAULT 'Visible',
  CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UpdatedAt DATETIME NULL,
  INDEX idx_event_status (EventId, Status),
  FOREIGN KEY (EventId) REFERENCES events(Id) ON DELETE CASCADE,
  FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_turkish_ci;

-- =============================================
-- 10) EVENT REACTIONS
-- =============================================
CREATE TABLE IF NOT EXISTS event_reactions (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  EventId INT NOT NULL,
  UserId INT NOT NULL,
  Type VARCHAR(20) NOT NULL DEFAULT 'Like',
  CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UNIQUE KEY uq_event_user_type (EventId, UserId, Type),
  FOREIGN KEY (EventId) REFERENCES events(Id) ON DELETE CASCADE,
  FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_turkish_ci;

-- =============================================
-- 11) EVENT REGISTRATIONS
-- =============================================
CREATE TABLE IF NOT EXISTS event_registrations (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  EventId INT NOT NULL,
  UserId INT NOT NULL,
  Status VARCHAR(20) NOT NULL DEFAULT 'Registered',
  CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UNIQUE KEY uq_event_user (EventId, UserId),
  FOREIGN KEY (EventId) REFERENCES events(Id) ON DELETE CASCADE,
  FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_turkish_ci;

-- =============================================
-- 12) CLUB MEMBERSHIP APPLICATIONS
-- =============================================
CREATE TABLE IF NOT EXISTS club_membership_applications (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  UserId INT NOT NULL,
  ClubId INT NOT NULL,
  Status VARCHAR(20) NOT NULL DEFAULT 'Pending',
  Note VARCHAR(500) NULL,
  QrCode TEXT NULL,
  CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ReviewedAt DATETIME NULL,
  UNIQUE KEY uq_membership (ClubId, UserId),
  FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE,
  FOREIGN KEY (ClubId) REFERENCES clubs(Id) ON DELETE CASCADE
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_turkish_ci;

-- =============================================
-- 13) AUDIT LOGS
-- =============================================
CREATE TABLE IF NOT EXISTS audit_logs (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  ActorUserId INT NULL,
  Action VARCHAR(100) NOT NULL,
  EntityType VARCHAR(100) NOT NULL,
  EntityId INT NULL,
  MetaJson JSON NULL,
  CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (ActorUserId) REFERENCES users(Id) ON DELETE SET NULL
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_turkish_ci;

-- =============================================
-- 14) ANALYTICS EVENTS
-- =============================================
CREATE TABLE IF NOT EXISTS analytics_events (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  UserId INT NULL,
  AnonId VARCHAR(128) NULL,
  EventName VARCHAR(100) NOT NULL,
  EntityType VARCHAR(100) NOT NULL,
  EntityId INT NULL,
  MetaJson JSON NULL,
  CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  INDEX idx_event_entity (EventName, EntityType, EntityId),
  FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE SET NULL
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_turkish_ci;

-- =============================================
-- 15) NOTIFICATIONS
-- =============================================
CREATE TABLE IF NOT EXISTS notifications (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  UserId INT NOT NULL,
  Type VARCHAR(50) NOT NULL,
  PayloadJson JSON NOT NULL,
  CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ReadAt DATETIME NULL,
  INDEX idx_notifications_user_read (UserId, ReadAt),
  FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_turkish_ci;

-- =============================================
-- 16) SEED DATA
-- =============================================
INSERT INTO users (Id, Name, Email, PasswordHash, Role, CreatedAt) VALUES
(1, 'Ayse Demir', 'student@bugbusters.dev', '$2a$11$abcdefghijklmnopqrstuusvjCjxykWM6EVr4NLmYOZDEUJzz8Mcm', 'Student', '2025-01-15 00:00:00'),
(2, 'Ali Yilmaz', 'admin1@bugbusters.dev', '$2a$11$abcdefghijklmnopqrstuunexzIc3x8I4UQSfyinzLbbkD0PvhD6a', 'ClubAdmin', '2025-01-15 00:00:00'),
(3, 'Ece Koc', 'admin2@bugbusters.dev', '$2a$11$abcdefghijklmnopqrstuunexzIc3x8I4UQSfyinzLbbkD0PvhD6a', 'ClubAdmin', '2025-01-15 00:00:00'),
(4, 'Sks Yetkilisi', 'sks@bugbusters.dev', '$2a$11$abcdefghijklmnopqrstuutK6kgbr7Fw8OIjF7TSn68Opi3gTMwfq', 'SksAdmin', '2025-01-15 00:00:00'),
(5, 'Ops Admin', 'ops@bugbusters.dev', '$2a$11$abcdefghijklmnopqrstuuUV5XOpUP0aNxeeIpIwHSsKodZk708Ru', 'SuperAdmin', '2025-01-15 00:00:00');

INSERT INTO student_profiles (UserId, Faculty, Department, Bio, AvatarUrl) VALUES
(1, 'Muhendislik', 'Bilgisayar Muhendisligi', 'Yapay zeka ve topluluk etkinlikleriyle ilgileniyor.', NULL);

INSERT INTO clubs (Id, Name, Description, Category, Contact, LogoUrl, Status, CreatedAt) VALUES
(1, 'Yapay Zeka Kulubu', 'AI projeleri ve arastirmalar.', 'Teknoloji', 'ai@okul.com', NULL, 'Active', '2025-01-15 00:00:00'),
(2, 'Siber Guvenlik Kulubu', 'CTF ve siber savunma egitimleri.', 'Teknoloji', 'security@okul.com', NULL, 'Active', '2025-01-15 00:00:00'),
(3, 'Girisimcilik Kulubu', 'Startup ve networking etkinlikleri.', 'Is ve Girisim', 'giris@okul.com', NULL, 'Active', '2025-01-15 00:00:00');

INSERT INTO club_admins (Id, ClubId, UserId, RoleInClub, CreatedAt) VALUES
(1, 1, 2, 'Owner', '2025-01-15 00:00:00'),
(2, 2, 3, 'Owner', '2025-01-15 00:00:00');

INSERT INTO events (Id, ClubId, Title, Description, Location, StartAt, EndAt, Capacity, Status, CreatedAt) VALUES
(1, 1, 'AI 101 Atolyesi', 'Yeni baslayanlar icin temel AI workshop.', 'M1 Konferans Salonu', '2025-02-10 18:00:00', '2025-02-10 20:00:00', 60, 'Published', '2025-01-15 00:00:00'),
(2, 1, 'Model Degerlendirme Semineri', 'ML model performansi ve metrikler.', 'Lab 3', '2025-02-18 17:00:00', '2025-02-18 19:00:00', 40, 'Published', '2025-01-15 00:00:00'),
(3, 2, 'CTF Hazirlik Oturumu', 'Takim olusturma ve alistirmalar.', 'Siber Lab', '2025-02-12 18:00:00', '2025-02-12 20:00:00', 50, 'Published', '2025-01-15 00:00:00'),
(4, 3, 'Startup Pitch Night', 'Fikirlerinizi juriye sunun.', 'Etkinlik Salonu', '2025-02-20 18:30:00', '2025-02-20 21:00:00', 120, 'Published', '2025-01-15 00:00:00'),
(5, 3, 'Yatirimci ile Kahve', 'Yatirimci bulusmasi ve mentorluk.', 'Kulupler Binasi', '2025-02-25 16:00:00', '2025-02-25 17:30:00', 30, 'Published', '2025-01-15 00:00:00');

INSERT INTO club_membership_applications (Id, UserId, ClubId, Status, Note, QrCode, CreatedAt, ReviewedAt) VALUES
(1, 1, 1, 'Approved', 'Seeded membership', NULL, '2025-01-15 00:00:00', '2025-01-15 00:00:00');
