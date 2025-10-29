# Proje Kurulum Rehberi

Bu belge, projenin yerel ortamda çalıştırılması için gerekli adımları içermektedir.

## Gereksinimler

- MySQL
- .NET SDK
- Node.js ve npm

## Kurulum Adımları

### 1. MySQL Kurulumu

Eğer sisteminizde MySQL kurulu değilse, öncelikle MySQL'i indirip kurmanız gerekmektedir.

### 2. Veritabanı Yapılandırması

`backend` klasörü içindeki `.env` dosyasına veritabanı bağlantı bilgilerinizi girin:

```
DB_USER=kullanici_adi
DB_PASSWORD=sifre
```

### 3. Veritabanı Oluşturma

`database_creation.sql` dosyasındaki komutları çalıştırarak veritabanı tablolarını oluşturun.

### 4. Backend'i Başlatma

Backend klasörüne gidin ve API'yi başlatın:

```bash
cd backend
dotnet run
```

### 5. Frontend'i Başlatma

Yeni bir terminal penceresi açın, frontend klasörüne gidin ve gerekli paketleri yükleyip uygulamayı başlatın:

```bash
cd frontend
npm install
npm run dev
```

## Tamamlandı

Artık uygulama hazır ve kullanıma hazır durumda!

- Backend API varsayılan olarak çalışıyor olmalıdır
- Frontend uygulamasına tarayıcınızdan erişebilirsiniz
