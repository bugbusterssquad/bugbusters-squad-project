-- Veritabanı
CREATE DATABASE clubsdb
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_turkish_ci;

USE clubsdb;

-- Tablo
CREATE TABLE ogrenci_kulupleri (
  id   INT AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(100) NOT NULL UNIQUE
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_turkish_ci;

-- Örnek veriler (istersen düzenle/ekle)
INSERT INTO ogrenci_kulupleri (name) VALUES
  ('Yapay Zeka Kulübü'),
  ('Siber Güvenlik Kulübü'),
  ('Girişimcilik Kulübü');