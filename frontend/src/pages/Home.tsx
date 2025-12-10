import React, { useEffect, useState } from "react";
import ClubCard from "../components/ClubCard";
import { api } from "../services/api";
import type { Club } from "../types";

export default function Home() {
  const [clubs, setClubs] = useState<Club[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    (async () => {
      try {
        setLoading(true);
        const data = await api.listClubs();
        setClubs(data);
      } catch (e: any) {
        setError(e.message || "Hata oluştu");
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  return (
    <div>
      <div className="mb-6">
        <h2 className="text-2xl font-bold">Kulüpler</h2>
        <p className="text-gray-600 mt-1">İlgini çeken kulübe tıklayarak detaylarını inceleyebilirsin.</p>
      </div>

      {loading && <div>Yükleniyor...</div>}
      {error && <div className="text-red-500">{error}</div>}

      <div className="grid gap-4 grid-cols-1 sm:grid-cols-2 lg:grid-cols-3">
        {clubs.map((c) => <ClubCard key={c.id} club={c} />)}
      </div>
    </div>
  );
}
