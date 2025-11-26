import { useState, useEffect } from "react";

// --- Stiller (Daha temiz bir görünüm için) ---
const containerStyle: React.CSSProperties = {
  maxWidth: 420,
  margin: "0 auto",
  padding: "30px",
  borderRadius: "12px",
  boxShadow: "0 8px 24px rgba(0,0,0,0.15)",
  backgroundColor: "#1e1e1e", // Dark mode arka plan
  color: "#eee",
  position: "relative", // Kapat butonu için
};

const inputStyle: React.CSSProperties = {
  padding: "12px",
  borderRadius: "6px",
  border: "1px solid #444",
  backgroundColor: "#333",
  color: "white",
  fontSize: "14px",
  outline: "none",
};

const buttonPrimaryStyle: React.CSSProperties = {
  padding: "12px",
  borderRadius: "6px",
  border: "none",
  backgroundColor: "#007bff",
  color: "white",
  fontSize: "16px",
  fontWeight: "600",
  cursor: "pointer",
  transition: "background-color 0.2s",
};

const buttonSecondaryStyle: React.CSSProperties = {
  background: "none",
  border: "none",
  color: "#007bff",
  cursor: "pointer",
  textDecoration: "underline",
  fontSize: "14px",
  padding: "5px",
};

// --- Bileşen Props Tanımı ---
type AuthProps = {
  onLoginSuccess?: (user: any) => void;
  // Yeni eklenenler (Giriş sonrası şifre değiştirme için)
  isModalMode?: boolean;
  prefilledEmail?: string;
  onCloseModal?: () => void;
};

export default function Auth({
  onLoginSuccess,
  isModalMode = false,
  prefilledEmail = "",
  onCloseModal,
}: AuthProps) {
  // Eğer modal modundaysak direkt 'reset' ekranıyla başla, yoksa 'login'
  const [view, setView] = useState<"login" | "register" | "reset">(
    isModalMode ? "reset" : "login"
  );
  const [form, setForm] = useState({ name: "", email: "", password: "" });
  const [message, setMessage] = useState<{ text: string; type: "success" | "error" } | null>(null);
  const [loading, setLoading] = useState(false);

  // Eğer dışarıdan email gelirse formu doldur
  useEffect(() => {
    if (prefilledEmail) {
      setForm((prev) => ({ ...prev, email: prefilledEmail }));
    }
  }, [prefilledEmail]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage(null);
    setLoading(true);

    let url = "";
    let body = {};

    if (view === "login") {
      url = "/api/auth/login";
      body = { email: form.email, password: form.password };
    } else if (view === "register") {
      url = "/api/auth/register";
      body = { ...form };
    } else {
      url = "/api/auth/reset-password";
      body = { email: form.email, newPassword: form.password };
    }

    try {
      const res = await fetch(url, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body),
      });

      const data = await res.json();

      if (!res.ok) throw new Error(data.message || data || "Bir hata oluştu");

      if (view === "login" && onLoginSuccess) {
        onLoginSuccess(data);
      } else {
        setMessage({
          text: data.message || "İşlem başarılı!",
          type: "success",
        });
        // Başarılı kayıt sonrası girişe yönlendir (modal değilse)
        if (view === "register" && !isModalMode) {
          setTimeout(() => setView("login"), 1500);
        }
        // Başarılı şifre değişimi sonrası formu temizle
        if (view === "reset") {
           setForm({...form, password: ""});
        }
      }
    } catch (err: any) {
      setMessage({ text: err.message, type: "error" });
    } finally {
        setLoading(false);
    }
  };

  // Form başlığını belirle
  let title = "";
  if (view === "login") title = "Giriş Yap";
  else if (view === "register") title = "Yeni Hesap Oluştur";
  else title = isModalMode ? "Şifre Değiştir" : "Şifre Sıfırla";

  return (
    <div style={containerStyle}>
      {/* Modal kapatma butonu (X) */}
      {isModalMode && onCloseModal && (
        <button
          onClick={onCloseModal}
          style={{
            position: "absolute", right: 15, top: 15, background: "none", border: "none", color: "#aaa", fontSize: 20, cursor: "pointer"
          }}
        >
          ✕
        </button>
      )}

      <h2 style={{ textAlign: "center", marginBottom: "25px", marginTop: 0 }}>{title}</h2>

      {message && (
        <div
          style={{
            padding: "10px",
            borderRadius: "6px",
            marginBottom: "20px",
            backgroundColor: message.type === "error" ? "rgba(220, 53, 69, 0.2)" : "rgba(40, 167, 69, 0.2)",
            color: message.type === "error" ? "#ff6b6b" : "#51cf66",
            textAlign: "center",
            fontSize: "14px"
          }}
        >
          {message.text}
        </div>
      )}

      <form onSubmit={handleSubmit} style={{ display: "flex", flexDirection: "column", gap: "15px" }}>
        {view === "register" && (
          <input
            type="text"
            placeholder="Adınız Soyadınız"
            value={form.name}
            onChange={(e) => setForm({ ...form, name: e.target.value })}
            required
            style={inputStyle}
          />
        )}
        
        <input
          type="email"
          placeholder="E-posta Adresi"
          value={form.email}
          // Eğer email dışarıdan doldurulmuşsa (giriş yapmış kullanıcı) değiştirilemesin
          disabled={!!prefilledEmail && isModalMode}
          onChange={(e) => setForm({ ...form, email: e.target.value })}
          required
          style={{...inputStyle, opacity: (!!prefilledEmail && isModalMode) ? 0.6 : 1 }}
        />

        <input
          type="password"
          placeholder={view === "reset" ? "Yeni Şifreniz" : "Şifreniz"}
          value={form.password}
          onChange={(e) => setForm({ ...form, password: e.target.value })}
          required
          style={inputStyle}
        />
        
        <button 
            type="submit" 
            disabled={loading}
            style={{...buttonPrimaryStyle, opacity: loading ? 0.7 : 1}}
        >
          {loading ? "İşleniyor..." : (view === "login" ? "Giriş" : view === "register" ? "Kayıt Ol" : "Şifreyi Güncelle")}
        </button>
      </form>

      {/* Modal modunda değilsek alt navigasyon linklerini göster */}
      {!isModalMode && (
        <div style={{ marginTop: "25px", display: "flex", justifyContent: "center", gap: "15px", flexWrap: "wrap" }}>
          {view !== "login" && (
            <button onClick={() => { setView("login"); setMessage(null); }} style={buttonSecondaryStyle}>
              Giriş Yap
            </button>
          )}
          {view !== "register" && (
            <button onClick={() => { setView("register"); setMessage(null); }} style={buttonSecondaryStyle}>
              Kayıt Ol
            </button>
          )}
          {view !== "reset" && (
            <button onClick={() => { setView("reset"); setMessage(null); }} style={buttonSecondaryStyle}>
              Şifremi Unuttum
            </button>
          )}
        </div>
      )}
    </div>
  );
}
