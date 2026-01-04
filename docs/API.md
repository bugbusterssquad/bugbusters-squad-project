# API Dokümantasyonu (v1)

Bu doküman, BugBusters GO backend API uçlarını ve temel kullanım kurallarını özetler.

## Genel

- **Base URL:** `http://localhost:5084` (varsayılan, yerelde değişebilir)
- **Auth:** JWT Bearer (`Authorization: Bearer <token>`)
- **Roles:** `Student`, `ClubAdmin`, `SksAdmin`, `SuperAdmin`
- **Pagination:** `page`, `pageSize` query paramları; toplam kayıt sayısı `X-Total-Count` header'ında döner.
- **Rate limit:** `auth` (login/register) ve `write` (kritik yazma) uçları limitlidir.

## Auth

- `POST /api/auth/register` (public)
  - body: `{ "name": "...", "email": "...", "password": "..." }`
- `POST /api/auth/login` (public)
  - body: `{ "email": "...", "password": "..." }`
  - response: `{ "token": "...", "user": { "id": 1, "name": "...", "email": "...", "role": "Student" } }`
- `POST /api/auth/logout` (auth)
- `GET /api/auth/me` (auth)

## Öğrenci / Public

- `GET /api/clubs` (public) — `search`, `category`, `page`, `pageSize`
- `GET /api/clubs/options` (public)
- `GET /api/clubs/{id}` (public)
- `GET /api/events` (public) — `clubId`, `start`, `end`, `page`, `pageSize`
- `GET /api/events/{id}` (public)
- `GET /api/events/{eventId}/comments` (public)
- `GET /api/events/{eventId}/reactions` (public)

## Profil & Bildirim

- `GET /api/profile/me` (auth)
- `PATCH /api/profile/me` (auth)
  - body: `{ "name": "...", "faculty": "...", "department": "...", "bio": "...", "avatarUrl": "..." }`
- `GET /api/notifications` (auth)
- `PATCH /api/notifications/{id}/read` (auth)

## Kulüp Başvuruları (Öğrenci)

- `POST /api/clubs/{clubId}/applications` (Student)
  - body: `{ "note": "..." }`
- `GET /api/club-applications/me` (auth)

## Etkinlik Kayıtları (Öğrenci)

- `POST /api/events/{eventId}/registrations` (Student)
- `GET /api/events/{eventId}/registrations/me` (auth)
- `DELETE /api/events/{eventId}/registrations/me` (auth)

## Club Admin

- `GET /api/admin/clubs/mine` (ClubAdmin/SuperAdmin)
- `GET /api/admin/clubs/{clubId}/events` (ClubAdmin/SuperAdmin)
- `POST /api/clubs/{clubId}/events` (ClubAdmin/SuperAdmin)
- `PATCH /api/events/{id}` (ClubAdmin/SuperAdmin)
- `GET /api/admin/clubs/{clubId}/announcements` (ClubAdmin/SuperAdmin)
- `POST /api/clubs/{clubId}/announcements` (ClubAdmin/SuperAdmin)
- `PATCH /api/announcements/{id}` (ClubAdmin/SuperAdmin)
- `GET /api/clubs/{clubId}/applications` (ClubAdmin/SuperAdmin)
- `PATCH /api/club-applications/{id}` (ClubAdmin/SuperAdmin)
- `GET /api/events/{eventId}/registrations` (ClubAdmin/SuperAdmin)
- `POST /api/events/{eventId}/documents` (ClubAdmin/SuperAdmin, `multipart/form-data`)
- `GET /api/events/{eventId}/documents` (ClubAdmin/SuperAdmin/SksAdmin)

## SKS

- `POST /api/sks/club-applications` (ClubAdmin/SuperAdmin)
  - body: `{ "clubId": 1 }`
- `GET /api/sks/club-applications` (SksAdmin/SuperAdmin) — `status` (Pending/Approved/Rejected)
- `PATCH /api/sks/club-applications/{id}` (SksAdmin/SuperAdmin)
  - body: `{ "status": "Approved|Rejected", "reviewNote": "..." }`
- `GET /api/sks/event-documents` (SksAdmin/SuperAdmin) — `status`
- `PATCH /api/sks/event-documents/{id}` (SksAdmin/SuperAdmin)
  - body: `{ "status": "Approved|Rejected", "reviewNote": "..." }`
- `GET /api/event-documents/{id}/download` (ClubAdmin/SuperAdmin/SksAdmin)
- `GET /api/admin/stats` (SksAdmin/SuperAdmin)
- `GET /api/admin/stats?format=csv` (SksAdmin/SuperAdmin, CSV export)
- `GET /api/admin/view-stats` (SksAdmin/SuperAdmin)

## Sosyal

- `POST /api/events/{eventId}/comments` (Student/ClubAdmin/SuperAdmin)
  - body: `{ "body": "..." }`
- `PATCH /api/comments/{id}/hide` (ClubAdmin/SuperAdmin)
- `DELETE /api/comments/{id}` (auth)
- `POST /api/events/{eventId}/reactions` (auth) — like/unlike toggle

## Örnek İstekler

```json
POST /api/auth/login
{
  "email": "student@bugbusters.dev",
  "password": "Student123!"
}
```

```json
POST /api/clubs/1/events
{
  "title": "Kariyer Günü",
  "description": "Konuk konuşmacılar ile seminer.",
  "location": "Salon A",
  "startAt": "2026-01-10T10:00:00Z",
  "endAt": "2026-01-10T12:00:00Z",
  "capacity": 80
}
```
