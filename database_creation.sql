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

CREATE TABLE kullanici (
  id INT AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(100) NOT NULL
  email VARCHAR(100) NOT NULL UNIQUE
  password VARCHAR(100) NOT NULL 
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_turkish_ci;

INSERT INTO ogrenci_kulupleri (name) VALUES
  ('Yapay Zeka Kulübü'),
  ('Siber Güvenlik Kulübü'),
  ('Girişimcilik Kulübü');

INSERT INTO kullanici (name, email, password) 
VALUES 
    ('Ayşe Demir', 'ayse@mail.com', 'ayse123'),
    ('Mehmet Öztürk', 'mehmet@test.com', 'mehmet456'),
    ('Zeynep Kaya', 'zeynep@ornek.com', 'sifre789');
