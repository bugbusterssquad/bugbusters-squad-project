# RUNBOOK

## Yerel Çalıştırma

- Gereksinimler: .NET 8 SDK, Node.js 18+, MySQL 8+
- Tek komut: `./scripts/dev.sh`
- Manuel:
  - Backend: `dotnet run` (`backend/`)
  - Frontend: `npm install` + `npm run dev` (`frontend/`)

## Ortam Değişkenleri (backend/.env)

- `DB_USER`, `DB_PASS`
- `API_ADDR` (CORS için virgülle birden fazla domain)
- `JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_SECRET`, `JWT_EXP_MINUTES`
- `MIGRATE_ON_STARTUP` (`true`/`false`)
- `NOTIFICATION_WEBHOOK_URL` (opsiyonel email/push köprüsü)

## Health Check

- `GET /health`
- `GET /ready`

## Loglama ve Hata İzleme

- Her istek için `X-Request-Id` üretimi ve structured log (method/path/status/userId) yazılır.
- Beklenmeyen hatalarda 500 + requestId döner.

## Migration & Seed

- Migration oluşturma: `dotnet ef migrations add <Name>` (`backend/`)
- Migration uygulama: `dotnet ef database update`
- Manuel kurulum: `MIGRATE_ON_STARTUP=false` + `database_creation.sql`
- Seed data EF Core üzerinden otomatik eklenir.

## Backup / Restore (MySQL)

- Yedek: `mysqldump -u <user> -p clubsdb > backup.sql`
- Geri yükleme: `mysql -u <user> -p clubsdb < backup.sql`

## Dosya Saklama (Etkinlik Belgeleri)

- Dosyalar: `backend/Storage/event-documents/<eventId>/`
- İzinler: PDF/PNG/JPEG, maksimum 10MB
- Prod’da bu klasörü düzenli yedekleyin.

## Olay Müdahalesi (Checklist)

- Uygulama logları (backend stdout)
- DB bağlantısı ve disk doluluğu
- JWT secret rotasyonu (auth sorunlarında)
- Storage klasörü ve izinler
- Son migration / seed değişiklikleri

## Rollback Notu

- Kritik hata durumunda DB yedeğini geri yükleyin.
- Uygulama sürümünü bir önceki release’e döndürün.

## Testler

- Unit/Integration: `dotnet test StudentClubs.sln`
- Frontend lint/build: `npm run lint` + `npm run build` (frontend/)
- E2E smoke (Playwright):
  - `npm run test:e2e` (frontend/)
  - Gereksinim: `npx playwright install`
  - Not: Backend + frontend çalışır durumda olmalı. E2E senaryoları seed verileriyle ve temiz DB ile çalışır.
