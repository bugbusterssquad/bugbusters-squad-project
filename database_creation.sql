CREATE DATABASE clubsdb
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_turkish_ci;

USE clubsdb;

-- =============================================
-- 1) KULÜP TABLOSU
-- =============================================
CREATE TABLE ogrenci_kulupleri (
  id INT AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(100) NOT NULL UNIQUE,
  mission TEXT NULL,
  management TEXT NULL,
  contact VARCHAR(255) NULL
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_turkish_ci;

-- =============================================
-- 2) KULLANICI TABLOSU
-- =============================================
CREATE TABLE kullanici (
  id INT AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(100) NOT NULL,
  email VARCHAR(100) NOT NULL UNIQUE,
  password VARCHAR(100) NOT NULL
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_turkish_ci;

-- =============================================
-- 3) ÜYELİK TABLOSU (yeni)
-- =============================================
CREATE TABLE kulup_uyelikleri (
    id INT AUTO_INCREMENT PRIMARY KEY,

    user_id INT NOT NULL,
    club_id INT NOT NULL,

    status ENUM('pending', 'approved', 'rejected') DEFAULT 'pending',
    qr_code TEXT NULL,

    FOREIGN KEY (user_id) REFERENCES kullanici(id) ON DELETE CASCADE,
    FOREIGN KEY (club_id) REFERENCES ogrenci_kulupleri(id) ON DELETE CASCADE
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_turkish_ci;

-- =============================================
-- 4) ÖRNEK VERİLER
-- =============================================
INSERT INTO ogrenci_kulupleri (name, mission, management, contact) VALUES
('Yapay Zeka Kulübü', 'AI projeleri ve araştırmalar', 'Başkan: Ali Yılmaz', 'ai@okul.com'),
('Siber Güvenlik Kulübü', 'CTF ve siber savunma eğitimleri', 'Başkan: Ece Koç', 'security@okul.com'),
('Girişimcilik Kulübü', 'Startup ve networking etkinlikleri', 'Başkan: Mehmet Arslan', 'giris@okul.com');

INSERT INTO kullanici (name, email, password) VALUES
('Ayşe Demir', 'ayse@mail.com', 'ayse123'),
('Mehmet Öztürk', 'mehmet@test.com', 'mehmet456'),
('Zeynep Kaya', 'zeynep@ornek.com', 'sifre789');

-- =============================================
-- 5) Üyelik örnek veri
-- =============================================

INSERT INTO kulup_uyelikleri (user_id, club_id, status, qr_code) VALUES
(1, 1, 'approved', 'BASE64_QR_KODU_BURAYA_GELECEK'),
(2, 2, 'pending', NULL),
(3, 3, 'rejected', NULL);
