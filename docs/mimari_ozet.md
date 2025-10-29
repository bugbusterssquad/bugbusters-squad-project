# Mimari Özeti - Üniversite Öğrenci Kulübü Uygulaması (Sprint 0)

**Takım:** Bugbusters Squad
**Tarih:** 29 Ekim 2025

Bu doküman, Sprint 0 hedefi olan "Uçtan uca çalışan minimal akış (DB → API → UI)" için kurulan iskelet projenin teknik mimarisini ve veri akışını özetlemektedir.

Bu özet, "Proje Dokümantasyonu ve Kurulum Talimatları" task'i kapsamında hazırlanmıştır.

## 1. Seçilen Teknoloji Yığını

* **Backend (API):** C# (.NET Web API)
* **Frontend (UI):** TypeScript (React)
* **Veritabanı (DB):** MySQL
* **Süreç Yönetimi:** Jira
* **Versiyon Kontrol:** Git (GitHub)

## 2. Mimari Felsefesi ve Veri Akışı

Projemiz, "Katmanlı Mimari" felsefesine (Separation of Concerns) dayanmaktadır.

### Backend Mimarisi

Backend, "Backend Katmanlı Mimari Oluşturma" task'inde belirtildiği gibi 3 ana katmandan oluşur:

1.  **Controller Katmanı:** Dış dünyadan (Frontend'den) gelen HTTP isteklerini karşılar. `Servis Katmanı`'nı çağırır.
2.  **Service Katmanı:** Projenin iş mantığını (business logic) içerir. Doğrudan veritabanıyla konuşmaz, `Repository Katmanı`'nı çağırır.
3.  **Repository (DAO) Katmanı:** Doğrudan veritabanı (MySQL) ile konuşan tek katmandır. Veritabanından veriyi okur.

### Uçtan Uca Veri Akış Diyagramı (DB → API → UI)

```mermaid
flowchart LR
  %% --- Katmanlar ---
  subgraph UI["UI / Frontend - React + TypeScript"]
    direction TB
    U["KULLANICI (Tarayıcı)"]
    BNR["SystemAnnouncementBanner"]
    FSVC["Frontend Service (API Client)"]
  end

  subgraph BE["Backend - .NET Web API (Katmanlı)"]
    direction TB
    CTRL["Controller<br/>SystemMessagesController"]
    SRV["Service<br/>SystemMessageService"]
    REPO["Repository (DAO)<br/>SystemMessageRepository"]
  end

  subgraph DB["Database - MySQL"]
    direction TB
    TBL["SystemMessages Tablosu"]
  end

  %% --- Akış ---
  U -->|"1) Sayfa yüklenir"| BNR
  BNR -->|"2) Duyuruyu getir()"| FSVC
  FSVC -->|"3) GET /api/system-message"| CTRL
  CTRL -->|"4) Getir()"| SRV
  SRV  -->|"5) Aktif duyuru isteği"| REPO
  REPO -->|"6) SQL sorgusu"| TBL
  TBL -->|"7) Sorgu sonucu"| HAS{"Aktif duyuru bulundu mu?"}
  HAS -- "Evet" --> OK["200 OK<br/>Çok yakında hizmetinizdeyiz"]
  HAS -- "Hayır" --> NF["404 Not Found<br/>Aktif bir sistem duyurusu bulunamadı"]
  REPO -.-> ERR["500 Internal Server Error<br/>Sistem duyurusu yüklenemedi"]
  OK --> CTRL
  NF --> CTRL
  ERR --> CTRL
  CTRL -->|"8) HTTP Response (JSON)"| FSVC
  FSVC -->|"9) Parse"| BNR
  BNR -->|"10) Mesaj görünür"| U

  classDef layer fill:#f6f8fa,stroke:#adb5bd,stroke-width:1px,color:#111;
  classDef ok fill:#e8f5e9,stroke:#66bb6a,color:#1b5e20;
  classDef nf fill:#fff8e1,stroke:#ffb74d,color:#e65100;
  classDef err fill:#ffebee,stroke:#e57373,color:#b71c1c;
  class UI,BE,DB layer;
  class OK ok;
  class NF nf;
  class ERR err;
```

## 3. Kullanılan API Endpoint ve Örnek JSON

Sprint 0 kapsamında sadece bir adet API endpoint'i hazırlanmıştır.

* **Metot:** `GET`
* **Adres (Endpoint):** `/api/system-message`
* **Açıklama:** Veritabanındaki (MySQL) `SystemMessages` tablosundan aktif olan duyuru metnini (`messageText`) getirir.

---

### Örnek Yanıtlar (JSON)

"Hata Yönetimi" ve "Kabul Kriterleri" göz önünde bulundurularak 3 durum da ele alınmıştır:

**Durum 1: Duyuru bulunduğunda (Başarılı - 200 OK)**
*Kabul Kriteri: Sistem Mesajının Görünürlüğü*

```json
{
  "messageText": "Çok yakında hizmetinizdeyiz"
}
```
**Durum 2: Aktif duyuru bulunamadığında (Bulunamadı - 404 Not Found) Kabul Kriteri: Mesaj Olmadığında**
```json
{
  "error": "Aktif bir sistem duyurusu bulunamadı."
}
```
**Durum 3: Sunucu hatası oluştuğunda (İç Sunucu Hatası - 500 Internal Server Error) Kabul Kriteri: Hata Durumunda**
```json
{
  "error": "Sistem duyurusu yüklenemedi"
}
