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
flowchart TD
  %% === YTÜ Sprint 0 - Minimal Akış (DB → API → UI) ===

  %% === UI Katmanı ===
  subgraph UI["UI / Frontend - React + TypeScript"]
    U["KULLANICI (Tarayıcı)"]
    BNR["SystemAnnouncementBanner Bileşeni"]
    FSVC["Frontend Service (API Client)"]
  end

  %% === Backend Katmanı ===
  subgraph BE["Backend (.NET Web API) - Katmanlı Mimari"]
    CTRL["Controller Katmanı<br/>SystemMessagesController"]
    SRV["Service Katmanı<br/>SystemMessageService"]
    REPO["Repository (DAO) Katmanı<br/>SystemMessageRepository"]
  end

  %% === Veritabanı Katmanı ===
  subgraph DB["MySQL Veritabanı"]
    TBL["SystemMessages Tablosu"]
  end

  %% === Ana Akış ===
  U -->|"1) Sayfa yüklenir"| BNR
  BNR -->|"2) Duyuruyu getir()"| FSVC
  FSVC -->|"3) HTTP GET /api/system-message"| CTRL
  CTRL -->|"4) Getir() metodu çağrılır"| SRV
  SRV -->|"5) Aktif duyuru isteği yapılır"| REPO
  REPO -->|"6) SQL sorgusu:<br/>SELECT * FROM SystemMessages<br/>WHERE isActive=1<br/>ORDER BY createdAt DESC<br/>LIMIT 1"| TBL

  %% === Karar Noktası ===
  TBL -->|"7) Sorgu sonucu döner"| HAS{"Aktif duyuru bulundu mu?"}

  HAS -- "Evet" --> OK["✅ 200 OK<br/>Çok yakında hizmetinizdeyiz"]
  HAS -- "Hayır" --> NF["⚠️ 404 Not Found<br/>Aktif bir sistem duyurusu bulunamadı"]

  %% === Hata Akışı ===
  REPO -.->|Veritabanı hatası| ERR["❌ 500 Internal Server Error<br/>Sistem duyurusu yüklenemedi"]
  SRV -.->|İş mantığı hatası| ERR
  CTRL -.->|Global Exception Middleware| ERR

  %% === Geri Dönüş Akışı ===
  OK --> CTRL
  NF --> CTRL
  ERR --> CTRL
  CTRL -->|"8) HTTP Response (JSON)"| FSVC
  FSVC -->|"9) JSON parse edilir"| BNR
  BNR -->|"10) Mesaj ekranda gösterilir"| U

  %% === Stil Tanımları ===
  classDef layer fill:#f6f8fa,stroke:#adb5bd,stroke-width:1px,color:#111;
  classDef pos fill:#e8f5e9,stroke:#66bb6a,color:#1b5e20;
  classDef neg fill:#ffebee,stroke:#e57373,color:#b71c1c;
  classDef warn fill:#fff8e1,stroke:#ffb74d,color:#e65100;

  class UI,BE,DB layer;
  class OK pos;
  class NF warn;
  class ERR neg;

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
