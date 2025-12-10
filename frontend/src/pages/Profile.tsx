import React, { useEffect, useState } from "react";
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

export default function Profile() {
  const user = getUser();
  const navigate = useNavigate();
  const [membership, setMembership] = useState<any | null>(null);
  const [loading, setLoading] = useState(false);
  const [err, setErr] = useState<string | null>(null);

  useEffect(() => {
    if (!user) {
      navigate("/login");
      return;
    }
    (async () => {
      try {
        setLoading(true);
        const data = await api.getUserMembership(user.id);
        setMembership(data);
      } catch (e: any) {
        setErr(e.message || null);
      } finally {
        setLoading(false);
      }
    })();
  }, [user, navigate]);

  if (!user) return null;

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div className="bg-white p-6 rounded-md shadow">
        <h2 className="text-xl font-semibold">Profil</h2>
        <p className="mt-2 text-gray-700"><strong>Ad:</strong> {user.name}</p>
        <p className="mt-1 text-gray-700"><strong>Email:</strong> {user.email}</p>
      </div>

      <div className="bg-white p-6 rounded-md shadow">
        <h3 className="text-lg font-semibold">Dijital Üyelik Kartı</h3>
        <p className="text-gray-600 mt-2">Onaylanmış bir üyeliğin varsa buradan dijital kartını (QR) görebilirsin.</p>

        {loading && <div>Yükleniyor...</div>}
        {err && <div className="text-red-500">{err}</div>}

        {!loading && !membership && (
          <div className="mt-4">
            <p className="text-gray-700">Onaylanmış üyeliğin bulunmuyor.</p>
            <button onClick={() => navigate("/")} className="mt-3 px-4 py-2 bg-accent text-white rounded">Kulüpleri Gör</button>
          </div>
        )}

        {membership && (
          <div className="mt-4 flex flex-col gap-3 items-start md:flex-row md:items-center">
            <div>
              <p className="font-semibold">Status: <span className="capitalize">{membership.status}</span></p>
              <p className="text-sm text-gray-600">Kulüp ID: {membership.clubId}</p>
            </div>
            {membership.qrCodeBase64 && (
              <div className="mt-2">
                <img
                  src={`data:image/png;base64,${membership.qrCodeBase64}`}
                  alt="Dijital Üye Kartı"
                  className="w-40 h-40 object-contain border rounded"
                />
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}
