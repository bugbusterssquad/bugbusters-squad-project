import { useEffect, useState } from "react";
import Auth from "./Auth";

type ClubDto = { name: string; message: string };
type User = { id: number; name: string; email: string };

export default function App() {
  const [user, setUser] = useState<User | null>(null);
  const [data, setData] = useState<ClubDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [showResetModal, setShowResetModal] = useState(false);

  const fetchClubs = async () => {
    try {
      const res = await fetch("/api/clubs");
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      const json: ClubDto[] = await res.json();
      setData(json);
    } catch (e: any) {
      setError(e.message ?? String(e));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (user) {
      fetchClubs();
    }
  }, [user]);

  // --- Giriş Yapılmamış Durum ---
  if (!user) {
    return (
      // Giriş ekranını dikey ve yatay olarak ortala
      <main style={{ 
        minHeight: "100vh", 
        display: "flex", 
        alignItems: "center", 
        justifyContent: "center", 
        backgroundColor: "#121212",
        padding: 20
      }}>
        <Auth onLoginSuccess={(u) => setUser(u)} />
      </main>
    );
  }

  // --- Giriş Yapılmış Ana Ekran ---
  
  // İçerik yüklenirken veya hata varsa gösterilecekler
  let content;
  if (loading && data.length === 0) content = <div>Yükleniyor…</div>;
  else if (error) content = <div>Hata: {error}</div>;
  else {
    content = (
      <ul style={{ listStyle: "none", padding: 0, marginTop: 16 }}>
        {data.map((c, i) => (
          <li
            key={i}
            style={{
              borderRadius: 12,
              padding: 20,
              marginBottom: 16,
              boxShadow: "0 2px 8px rgba(0,0,0,0.2)",
              backgroundColor: "#2a2a2a",
              border: "1px solid #3a3a3a"
            }}
          >
            <div style={{ fontWeight: 700, fontSize: 20, color: "#fff" }}>{c.name}</div>
            <div style={{ marginTop: 8, color: "#ccc", lineHeight: 1.5 }}>{c.message}</div>
          </li>
        ))}
      </ul>
    );
  }

  return (
    <main style={{ padding: "30px", maxWidth: 800, margin: "0 auto", backgroundColor: "#121212", minHeight: "100vh", color: "#eee" }}>
      {/* Üst Başlık ve Kullanıcı Menüsü */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 30, paddingBottom: 20, borderBottom: "1px solid #333" }}>
        <h1 style={{ margin: 0 }}>Öğrenci Kulüpleri</h1>
        <div style={{ textAlign: "right" }}>
          <div style={{ marginBottom: 8, fontWeight: 600 }}>{user.name}</div>
          <div style={{ display: "flex", gap: "10px" }}>
             {/* Yeni Şifre Değiştir Butonu */}
            <button 
              onClick={() => setShowResetModal(true)} 
              style={{ padding: "6px 12px", fontSize: "0.85em", cursor: "pointer", backgroundColor: "#444", color: "#eee", border: "none", borderRadius: 4 }}
            >
              Şifre Değiştir
            </button>
            <button 
              onClick={() => setUser(null)} 
              style={{ padding: "6px 12px", fontSize: "0.85em", cursor: "pointer", backgroundColor: "#c92a2a", color: "white", border: "none", borderRadius: 4 }}
            >
              Çıkış
            </button>
          </div>
        </div>
      </div>

      {content}

      {/* --- Şifre Değiştirme Modalı --- */}
      {/* Eğer showResetModal true ise, ekranın üzerine siyah yarı saydam bir katman ekle */}
      {showResetModal && (
        <div style={{
          position: "fixed", top: 0, left: 0, width: "100%", height: "100%",
          backgroundColor: "rgba(0,0,0,0.7)", // Arka planı karart
          display: "flex", alignItems: "center", justifyContent: "center", zIndex: 1000
        }}>
          {/* Auth bileşenini modal modunda render et */}
          <Auth 
            isModalMode={true}
            prefilledEmail={user.email} // Kullanıcının emailini gönder
            onCloseModal={() => setShowResetModal(false)} // Kapatma fonksiyonu
          />
        </div>
      )}
    </main>
  );
}
