import React, { useEffect, useState } from "react";
import { useParams } from "react-router";
import { api } from "../services/api";
import type { Club } from "../types";
import MembershipApply from "../components/MembershipApply";

export default function ClubDetail() {
  const { id } = useParams();
  const clubId = Number(id);
  const [club, setClub] = useState<Club | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!clubId) return;
    (async () => {
      try {
        setLoading(true);
        const data = await api.getClub(clubId);
        setClub(data);
      } catch (e: any) {
        setError(e.message || "Hata");
      } finally {
        setLoading(false);
      }
    })();
  }, [clubId]);

  if (!clubId) return <div>Geçersiz kulüp.</div>;

  return (
    <div>
      {loading && <div>Yükleniyor...</div>}
      {error && <div className="text-red-500">{error}</div>}
      {club && (
        <div className="space-y-6">
          <div className="bg-white p-6 rounded-md shadow">
            <h2 className="text-2xl font-bold">{club.name}</h2>
            <div className="mt-4 space-y-3">
              <div>
                <h4 className="font-semibold">Misyon</h4>
                <p className="text-gray-700">{club.mission ?? "Misyon bilgisi bulunmuyor."}</p>
              </div>
              <div>
                <h4 className="font-semibold">Yönetim</h4>
                <p className="text-gray-700">{club.management ?? "Yönetim bilgisi bulunmuyor."}</p>
              </div>
              <div>
                <h4 className="font-semibold">İletişim</h4>
                <p className="text-gray-700">{club.contact ?? "İletişim bilgisi bulunmuyor."}</p>
              </div>
            </div>
          </div>

          <MembershipApply clubId={club.id} />
        </div>
      )}
    </div>
  );
}
