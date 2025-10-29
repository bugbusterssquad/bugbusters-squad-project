# 🚀 Üniversite Öğrenci Kulübü Uygulaması (Bugbusters Squad)

Bu proje, "MTH4710 – Yazılım Sektöründe Çevik Dönüşüm ve Uygulama Pratikleri" dersi kapsamında geliştirilen bir üniversite öğrenci kulübü yönetim sistemidir.

Bu repo, Sprint 0 (İskelet Proje) görevini içermektedir.

## 🎯 Sprint 0 Hedefi

Uçtan uca çalışan minimal bir akış (DB → API → UI) oluşturmak. Veritabanından (MySQL) okunan bir sistem duyurusunu ("Çok yakında hizmetinizdeyiz") C# Backend API üzerinden TypeScript Frontend'de göstermek.

## 🛠️ Teknoloji Yığını

* **Backend:** C# (.NET 7 Web API)
* **Frontend:** TypeScript (React)
* **Database:** MySQL

## 🏃‍♂️ Proje Nasıl Çalıştırılır?

Proje `backend` ve `frontend` olmak üzere iki ana bölümden oluşur. Çalıştırmak için ikisinin de ayakta olması gerekir.

### 1. Backend (C# / .NET API)

Backend sunucusunu ayağa kaldırmak için:

1.  Projenin `backend/` klasörüne gidin.
2.  `backend/StudentClubs.Api/appsettings.json` dosyasını açın. `ConnectionStrings` bölümünü kendi yerel MySQL sunucunuzun (kullanıcı adı, şifre, port) bilgileriyle güncelleyin.
3.  (Varsa) Veritabanı tablolarını oluşturmak için terminalde `dotnet ef database update` komutunu çalıştırın.
4.  API sunucusunu başlatmak için terminalde `dotnet run` komutunu çalıştırın.
5.  Sunucu varsayılan olarak `http://localhost:5123` (veya benzeri bir portta) çalışmaya başlayacaktır.

### 2. Frontend (TypeScript / React)

Kullanıcı arayüzünü ayağa kaldırmak için:

1.  Yeni bir terminal açın ve projenin `frontend/` klasörüne gidin.
2.  Gerekli tüm paketleri kurmak için `npm install` komutunu çalıştırın.
3.  Uygulamayı başlatmak için `npm start` komutunu çalıştırın.
4.  Uygulama otomatik olarak tarayıcınızda `http://localhost:3000` adresinde açılacaktır. Ekranda sistem duyurusunu görmelisiniz.

## 📋 Jira Süreç Takibi

Projemizin tüm çevik planlaması, Story'ler, Task'ler ve Sub-task'ler Jira bordumuz üzerinden takip edilmektedir:

**https://bugbusterssquad.atlassian.net/jira/software/projects/SCRUM/boards/1**
