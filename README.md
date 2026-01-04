# 🚀 Üniversite Öğrenci Kulübü Uygulaması (BugBusters Squad)

Bu proje, **"MTH4710 – Yazılım Sektöründe Çevik Dönüşüm ve Uygulama Pratikleri"** dersi kapsamında geliştirilen, öğrencilerin kulüplere katılımını ve etkileşimini dijitalleştiren bir yönetim sistemidir.

## 📅 Proje Durumu

RoadMap.md kapsamındaki Sprint 0 → 1.2 özellikleri tamamlandı.

### ✅ Tamamlanan Kapsam
- Auth + profil (JWT)
- Kulüp/etkinlik listeleme + detay + sayfalama + analytics
- Kulüp admin paneli (etkinlik/duyuru/başvuru/kayıt yönetimi)
- SKS paneli (kulüp başvurusu + etkinlik belgesi)
- Waitlist + bildirimler + sosyal yorum/beğeni
- CI, migrations/seed, health endpoints, testler, dokümantasyon

## 🗄️ Veritabanı ve Dokümantasyon

Güncel şema ve migration notları için:
- `docs/DB.md`
- `database_creation.sql`

API referansı:
- `docs/API.md`

Operasyon/çalıştırma notları:
- `docs/RUNBOOK.md`
---

## 🛠️ Teknoloji Yığını

* **Backend:** C# (.NET 8 Web API)
* **Frontend:** TypeScript (React)
* **Database:** MySQL
* **ORM:** Entity Framework Core
* **Süreç Yönetimi:** Jira & GitHub

## 🏃‍♂️ Proje Nasıl Çalıştırılır?

Proje `backend` ve `frontend` olmak üzere iki ana bölümden oluşur. Güncel kod `main` branch'inde bulunmaktadır.

### Tek Komut (Opsiyonel)

```bash
./scripts/dev.sh
```

### 1. Backend (C# / .NET API)

Backend sunucusunu ayağa kaldırmak ve veritabanını güncellemek için:

1.  Projenin `backend/` klasörüne gidin.
2.  `backend/.env` dosyasındaki `DB_USER`, `DB_PASS`, `JWT_SECRET` değerlerini kendi yerel bilgilerinize göre güncelleyin (gerekirse `appsettings.json` içindeki sunucu/port/db adını düzenleyin).
3.  **Not:** Uygulama başlangıcında EF Core migrations otomatik uygulanır. Eğer `database_creation.sql` ile manuel kurulum yapacaksanız `MIGRATE_ON_STARTUP=false` ayarlayın:
    ```bash
    mysql -u root -p < database_creation.sql
    ```
4.  API sunucusunu başlatın:
    ```bash
    dotnet run
    ```
5.  Sunucu `http://localhost:5084` adresinde çalışacaktır.

### 2. Frontend (TypeScript / React)

Kullanıcı arayüzünü ayağa kaldırmak için:

1.  Yeni bir terminal açın ve `frontend/` klasörüne gidin.
2.  Paketleri yükleyin:
    ```bash
    npm install
    ```
3.  Uygulamayı başlatın:
    ```bash
    npm run dev
    ```
4.  Tarayıcınızda `http://localhost:5173` adresine giderek Login ekranını ve Kulüp Listesini görebilirsiniz.

## ✅ Testler

```bash
dotnet test StudentClubs.sln
```

```bash
cd frontend
npm run lint
npm run build
npm run test:e2e
```

> E2E testleri Playwright kullanır ve backend + frontend ayağa kalkmış olmalıdır. İlk kurulum için `npx playwright install` çalıştırılmalıdır.

## 🧷 Git Hook (Opsiyonel)

```bash
./scripts/setup-git-hooks.sh
```

## 🔐 Varsayılan Kullanıcılar (Seed)

- Öğrenci: `student@bugbusters.dev` / `Student123!`
- Kulüp Admin: `admin1@bugbusters.dev` / `ClubAdmin123!`
- Kulüp Admin: `admin2@bugbusters.dev` / `ClubAdmin123!`
- SKS Admin: `sks@bugbusters.dev` / `SksAdmin123!`
- Super Admin: `ops@bugbusters.dev` / `SuperAdmin123!`

## 📋 Jira Süreç Takibi

Projemizin tüm çevik planlaması, User Story'ler, Task'ler ve Sprint Burndown grafikleri Jira bordumuz üzerinden şeffaf bir şekilde takip edilmektedir:

🔗 **[BugBusters Jira Board](https://bugbusterssquad.atlassian.net/jira/software/projects/SCRUM/boards/1)**

---
*Geliştirici Ekip: BugBusters Squad*
