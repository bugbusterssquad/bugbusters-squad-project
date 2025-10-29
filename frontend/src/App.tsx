import { useEffect, useState } from "react";

type ClubDto = { name: string; message: string };

export default function App() {
  const [data, setData] = useState<ClubDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    (async () => {
      try {
        const res = await fetch("/api/clubs"); // proxy -> http://localhost:5084/api/clubs
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        const json: ClubDto[] = await res.json();
        setData(json);
      } catch (e: any) {
        setError(e.message ?? String(e));
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  if (loading) return <main style={{ padding: 24 }}>Yükleniyor…</main>;
  if (error) return <main style={{ padding: 24 }}>Hata: {error}</main>;

  return (
    <main style={{ padding: 24, maxWidth: 720, margin: "0 auto" }}>
      <h1>Öğrenci Kulüpleri</h1>

      <ul style={{ listStyle: "none", padding: 0, marginTop: 16 }}>
        {data.map((c, i) => (
          <li
            key={i}
            style={{
              border: "1px solid #e5e7eb",
              borderRadius: 12,
              padding: 16,
              marginBottom: 12,
              boxShadow: "0 1px 2px rgba(0,0,0,0.04)",
            }}
          >
            <div style={{ fontWeight: 600, fontSize: 18 }}>{c.name}</div>
            <div style={{ marginTop: 6 }}>{c.message}</div>
          </li>
        ))}
      </ul>
    </main>
  );
}
