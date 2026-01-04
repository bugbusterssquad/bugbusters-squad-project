# İlk Kurulum ve Çalıştırma Rehberi (Git ile)

Bu doküman, projeyi **git üzerinden ilk kez indirip** kendi bilgisayarında **ilk kez çalıştıracak** kişiler içindir.

## 1) Gereksinimler

- Git
- .NET 8 SDK
- Node.js 18+ (önerilen: 20)
- MySQL 8+

## 2) Projeyi İndir

```bash
git clone <repo-url>
cd bugbusters-squad-project
```

## 3) Veritabanı Hazırlığı

MySQL’de bir veritabanı oluştur:

```sql
CREATE DATABASE IF NOT EXISTS clubsdb CHARACTER SET utf8mb4 COLLATE utf8mb4_turkish_ci;
```

> DB adı varsayılan olarak `clubsdb`’dir. Değiştirmek istersen `backend/appsettings.json` içindeki connection string’i güncelle.

## 4) Backend Ortam Değişkenleri

`backend/.env.example` dosyasını kopyalayıp `backend/.env` oluştur:

```bash
cp backend/.env.example backend/.env
```

`backend/.env` içine MySQL kullanıcı ve şifreni yaz:

```
DB_USER=root
DB_PASS=your_db_password
API_ADDR=http://localhost:5173
MIGRATE_ON_STARTUP=true
```

> `MIGRATE_ON_STARTUP=true` ise uygulama açılışında migration’lar otomatik uygulanır.
> Manuel kurulum istersen `MIGRATE_ON_STARTUP=false` ve `database_creation.sql` kullanabilirsin.

## 5) Bağımlılıkları Kur

```bash
cd frontend
npm install
cd ..
```

## 6) Projeyi Çalıştır

### Seçenek A: Tek komut (önerilen)

```bash
./scripts/dev.sh
```

### Seçenek B: Ayrı terminallerde

Backend:
```bash
cd backend
dotnet run
```

Frontend:
```bash
cd frontend
npm run dev
```

## 7) Uygulamaya Erişim

- Frontend: `http://localhost:5173`
- Backend: `http://localhost:5084`
- Health: `http://localhost:5084/health`

## 8) Seed Kullanıcılar

- Öğrenci: `student@bugbusters.dev` / `Student123!`
- Kulüp Admin: `admin1@bugbusters.dev` / `ClubAdmin123!`
- Kulüp Admin: `admin2@bugbusters.dev` / `ClubAdmin123!`
- SKS Admin: `sks@bugbusters.dev` / `SksAdmin123!`
- Super Admin: `ops@bugbusters.dev` / `SuperAdmin123!`

## 9) İlk Test Akışı (Önerilen)

1) Öğrenci ile giriş yap → kulüp listesi ve etkinlikler görünür mü?
2) Etkinlik detayına gir → kayıt ol → bildirim geldi mi?
3) Kulüp admin ile giriş → yeni etkinlik oluştur → yayınla
4) SKS admin ile giriş → pending başvuru/belge onayla

## 10) Sık Karşılaşılan Sorunlar

- **Failed to fetch / CORS**: Frontend `http://localhost:5173`, backend `http://localhost:5084` olmalı.
- **DB bağlantı hatası**: `DB_USER` / `DB_PASS` kontrol et, MySQL çalışıyor mu bak.
- **Ekranda sürekli “Yükleniyor”**: Frontend sayfayı hard refresh (`Cmd+Shift+R`) ve `localStorage` temizleyip tekrar login ol.

## 11) Testler (Opsiyonel)

```bash
dotnet test StudentClubs.sln
```

```bash
cd frontend
npm run lint
npm run build
```

E2E (Playwright):
```bash
cd frontend
npx playwright install
npm run test:e2e
```
