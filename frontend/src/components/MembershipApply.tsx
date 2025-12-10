import React, { useState } from "react";
import { api } from "../services/api";
import { useNavigate } from "react-router";

function getUser() {
  const raw = localStorage.getItem("clubs_user");
  if (!raw) return null;
  try {
    return JSON.parse(raw);
  } catch {
    return null;
  }
}

export default function MembershipApply({ clubId }: { clubId: number }) {
  const [status, setStatus] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const user = getUser();

  async function apply() {
    if (!user) {
      navigate("/login");
      return;
    }

    try {
      setLoading(true);
      const res = await api.applyMembership(user.id, clubId);
      setStatus(res.message ?? "Başvuru yapıldı.");
    } catch (e: any) {
      setStatus(e.message || "Başvuru başarısız.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="bg-white p-6 rounded-md shadow">
      <h3 className="text-lg font-semibold">Bu kulübe üye olmak ister misin?</h3>
      <p className="text-gray-600 mt-2">Üyelik başvurusu göndererek kulüple iletişime geçersin. Onaylandığında dijital üye kartın profilinde görünecektir.</p>

      <div className="mt-4 flex items-center gap-3">
        <button onClick={apply} disabled={loading} className="px-4 py-2 bg-accent rounded shadow-sm">
          {loading ? "Gönderiliyor..." : "Üyelik Başvurusu Yap"}
        </button>
        <button onClick={() => navigate("/")} className="px-4 py-2 border rounded">Geri</button>
      </div>

      {status && <div className="mt-4 text-sm text-gray-700">{status}</div>}
    </div>
  );
}
