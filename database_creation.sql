-- Veritabanı oluştur
CREATE DATABASE clubsdb
CHARACTER SET utf8mb4
COLLATE utf8mb4_turkish_ci;

USE clubsdb;

-- Kullanıcı rolleri için enum tablosu
CREATE TABLE roller (
  id INT AUTO_INCREMENT PRIMARY KEY,
  rol_adi VARCHAR(50) NOT NULL UNIQUE,
  aciklama VARCHAR(255),
  olusturma_tarihi TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  INDEX idx_rol_adi (rol_adi)
) ENGINE=InnoDB
DEFAULT CHARSET=utf8mb4
COLLATE=utf8mb4_turkish_ci;

-- Kullanıcılar tablosu
CREATE TABLE kullanicilar (
  id INT AUTO_INCREMENT PRIMARY KEY,
  kullanici_adi VARCHAR(50) NOT NULL UNIQUE,
  email VARCHAR(100) NOT NULL UNIQUE,
  sifre_hash VARCHAR(255) NOT NULL,
  ad VARCHAR(50) NOT NULL,
  soyad VARCHAR(50) NOT NULL,
  telefon VARCHAR(20),
  rol_id INT DEFAULT 3,
  aktif BOOLEAN DEFAULT TRUE,
  olusturma_tarihi TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  guncelleme_tarihi TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  son_giris TIMESTAMP NULL,
  FOREIGN KEY (rol_id) REFERENCES roller(id) ON DELETE RESTRICT,
  INDEX idx_email (email),
  INDEX idx_kullanici_adi (kullanici_adi),
  INDEX idx_rol_id (rol_id)
) ENGINE=InnoDB
DEFAULT CHARSET=utf8mb4
COLLATE=utf8mb4_turkish_ci;

-- Kulüpler tablosu (geliştirilmiş)
CREATE TABLE ogrenci_kulupleri (
  id INT AUTO_INCREMENT PRIMARY KEY,
  kulup_adi VARCHAR(100) NOT NULL UNIQUE,
  aciklama TEXT,
  kurulus_tarihi DATE,
  yonetici_id INT,
  aktif BOOLEAN DEFAULT TRUE,
  olusturma_tarihi TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  guncelleme_tarihi TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  FOREIGN KEY (yonetici_id) REFERENCES kullanicilar(id) ON DELETE SET NULL,
  INDEX idx_kulup_adi (kulup_adi),
  INDEX idx_yonetici_id (yonetici_id)
) ENGINE=InnoDB
DEFAULT CHARSET=utf8mb4
COLLATE=utf8mb4_turkish_ci;


INSERT INTO roller (rol_adi, aciklama) VALUES
  ('admin', 'Sistem yöneticisi - tam yetki'),
  ('club_adim', 'Kulüp yöneticisi - kulüp işlemleri yetkisi'),
  ('student', 'Öğrenci - standart kullanıcı');


INSERT INTO kullanicilar (kullanici_adi, email, sifre_hash, ad, soyad, telefon, rol_id) VALUES
  -- Kulüp Yöneticileri
  ('ahmet.yilmaz', 'ahmet.yilmaz@universite.edu.tr', MD5('sifre123'), 'Ahmet', 'Yılmaz', '0532 111 2233', 2),
  ('zeynep.kaya', 'zeynep.kaya@universite.edu.tr', MD5('sifre123'), 'Zeynep', 'Kaya', '0533 222 3344', 2),
  ('mehmet.demir', 'mehmet.demir@universite.edu.tr', MD5('sifre123'), 'Mehmet', 'Demir', '0534 333 4455', 2),
  
  -- Öğrenciler
  ('ayse.celik', 'ayse.celik@ogrenci.edu.tr', MD5('sifre123'), 'Ayşe', 'Çelik', '0535 444 5566', 3),
  ('ali.ozturk', 'ali.ozturk@ogrenci.edu.tr', MD5('sifre123'), 'Ali', 'Öztürk', '0536 555 6677', 3),
  ('fatma.sahin', 'fatma.sahin@ogrenci.edu.tr', MD5('sifre123'), 'Fatma', 'Şahin', '0537 666 7788', 3),
  ('can.arslan', 'can.arslan@ogrenci.edu.tr', MD5('sifre123'), 'Can', 'Arslan', '0538 777 8899', 3),
  ('elif.yildirim', 'elif.yildirim@ogrenci.edu.tr', MD5('sifre123'), 'Elif', 'Yıldırım', '0539 888 9900', 3),
  ('burak.kurt', 'burak.kurt@ogrenci.edu.tr', MD5('sifre123'), 'Burak', 'Kurt', '0530 999 0011', 3);

INSERT INTO ogrenci_kulupleri (kulup_adi, aciklama, kurulus_tarihi, yonetici_id) VALUES
  ('Yapay Zeka Kulübü', 
   'Makine öğrenmesi, derin öğrenme ve yapay zeka teknolojileri üzerine çalışan öğrenci topluluğu', 
   '2022-09-15', 1),
  ('Siber Güvenlik Kulübü', 
   'Etik hackleme, ağ güvenliği ve siber tehdit analizi konularında eğitim ve projeler geliştiren kulüp', 
   '2022-10-01', 2),
  ('Girişimcilik Kulübü', 
   'Startup fikirleri geliştiren, iş planları hazırlayan ve girişimcilik ekosistemi ile bağlantı kuran topluluk', 
   '2023-02-20', 3);
