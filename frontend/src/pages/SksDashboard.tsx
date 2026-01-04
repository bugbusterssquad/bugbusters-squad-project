import { useEffect, useState } from "react";
import { useNavigate } from "react-router";
import { api } from "../services/api";
import { getUser } from "../services/auth";
import type { AdminStats, SksClubApplication, SksEventDocument } from "../types";
import { getErrorMessage } from "../utils/error";

export default function SksDashboard() {
  const user = getUser();
  const userId = user?.id ?? null;
  const userRole = user?.role ?? null;
  const navigate = useNavigate();

  const [clubApps, setClubApps] = useState<SksClubApplication[]>([]);
  const [documents, setDocuments] = useState<SksEventDocument[]>([]);
  const [stats, setStats] = useState<AdminStats | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [reviewNotes, setReviewNotes] = useState<Record<number, string>>({});

  useEffect(() => {
    if (!userId) {
      navigate("/login");
      return;
    }
    if (userRole !== "SksAdmin" && userRole !== "SuperAdmin") {
      navigate("/");
      return;
    }

    (async () => {
      try {
        setLoading(true);
        const [clubData, docData, statData] = await Promise.all([
          api.listSksClubApplications("Pending"),
          api.listSksEventDocuments("Pending"),
          api.getAdminStats()
        ]);
        setClubApps(clubData);
        setDocuments(docData);
        setStats(statData);
      } catch (e) {
        setError(getErrorMessage(e));
      } finally {
        setLoading(false);
      }
    })();
  }, [userId, userRole, navigate]);

  async function handleClubReview(id: number, status: "Approved" | "Rejected") {
    try {
      const note = reviewNotes[id];
      await api.reviewSksClubApplication(id, status, note);
      const data = await api.listSksClubApplications("Pending");
      setClubApps(data);
    } catch (e) {
      setError(getErrorMessage(e));
    }
  }

  async function handleDocumentReview(id: number, status: "Approved" | "Rejected") {
    try {
      const note = reviewNotes[id];
      await api.reviewSksEventDocument(id, status, note);
      const data = await api.listSksEventDocuments("Pending");
      setDocuments(data);
    } catch (e) {
      setError(getErrorMessage(e));
    }
  }

  async function handleDownload(id: number, fileName: string) {
    try {
      const blob = await api.downloadEventDocument(id);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = fileName;
      a.click();
      window.URL.revokeObjectURL(url);
    } catch (e) {
      setError(getErrorMessage(e));
    }
  }

  if (loading) return <div>Yükleniyor...</div>;

  return (
    <div className="max-w-5xl mx-auto space-y-8">
      <div className="bg-white p-6 rounded-md shadow">
        <h2 className="text-2xl font-bold">SKS Paneli</h2>
        <p className="text-gray-600 mt-1">Başvurular, belgeler ve özet metrikler</p>
        {error && <div className="text-red-500 mt-2">{error}</div>}
      </div>

      {stats && (
        <div className="grid gap-4 md:grid-cols-3">
          <div className="bg-white p-4 rounded shadow">
            <p className="text-sm text-gray-600">Toplam Kayıt</p>
            <p className="text-2xl font-bold mt-2">{stats.totalRegistrations}</p>
          </div>
          <div className="bg-white p-4 rounded shadow">
            <p className="text-sm text-gray-600">Kulüp Sayısı</p>
            <p className="text-2xl font-bold mt-2">{stats.clubs.length}</p>
          </div>
          <div className="bg-white p-4 rounded shadow">
            <p className="text-sm text-gray-600">DAU (Son 14 gün)</p>
            <p className="text-2xl font-bold mt-2">{stats.dauTrend.at(-1)?.count ?? 0}</p>
          </div>
        </div>
      )}

      {stats && (
        <div className="bg-white p-6 rounded-md shadow">
          <h3 className="text-lg font-semibold">Kulüp İstatistikleri</h3>
          <div className="mt-4 space-y-3">
            {stats.clubs.map((club) => (
              <div key={club.clubId} className="border rounded p-3 flex items-center justify-between">
                <div>
                  <p className="font-semibold">{club.clubName}</p>
                  <p className="text-sm text-gray-600">Etkinlik: {club.eventCount} · Kayıt: {club.registrationCount}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      <div className="bg-white p-6 rounded-md shadow">
        <h3 className="text-lg font-semibold">Kulüp Başvuruları (Pending)</h3>
        <div className="mt-4 space-y-3">
          {clubApps.length === 0 && <p className="text-gray-600">Bekleyen başvuru yok.</p>}
          {clubApps.map((app) => (
            <div key={app.id} className="border rounded p-4 space-y-2">
              <div>
                <p className="font-semibold">{app.clubName}</p>
                <p className="text-sm text-gray-600">Gönderen: {app.submittedByName} ({app.submittedByEmail})</p>
              </div>
              <input
                value={reviewNotes[app.id] ?? ""}
                onChange={(e) => setReviewNotes((prev) => ({ ...prev, [app.id]: e.target.value }))}
                className="w-full p-2 border rounded"
                placeholder="Review notu"
                aria-label="Kulüp başvurusu review notu"
              />
              <div className="flex items-center gap-2">
                <button
                  onClick={() => handleClubReview(app.id, "Approved")}
                  className="text-sm px-3 py-2 border rounded"
                  data-testid="sks-approve-club"
                >
                  Onayla
                </button>
                <button
                  onClick={() => handleClubReview(app.id, "Rejected")}
                  className="text-sm px-3 py-2 border rounded"
                  data-testid="sks-reject-club"
                >
                  Reddet
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>

      <div className="bg-white p-6 rounded-md shadow">
        <h3 className="text-lg font-semibold">Etkinlik Belgeleri (Pending)</h3>
        <div className="mt-4 space-y-3">
          {documents.length === 0 && <p className="text-gray-600">Bekleyen belge yok.</p>}
          {documents.map((doc) => (
            <div key={doc.id} className="border rounded p-4 space-y-2">
              <div>
                <p className="font-semibold">{doc.eventTitle}</p>
                <p className="text-sm text-gray-600">Kulüp: {doc.clubName}</p>
                <p className="text-sm text-gray-600">Dosya: {doc.fileName}</p>
              </div>
              <div className="flex items-center gap-2">
                <button
                  onClick={() => handleDownload(doc.id, doc.fileName)}
                  className="text-sm px-3 py-2 border rounded"
                >
                  İndir
                </button>
              </div>
              <input
                value={reviewNotes[doc.id] ?? ""}
                onChange={(e) => setReviewNotes((prev) => ({ ...prev, [doc.id]: e.target.value }))}
                className="w-full p-2 border rounded"
                placeholder="Review notu"
                aria-label="Etkinlik belgesi review notu"
              />
              <div className="flex items-center gap-2">
                <button
                  onClick={() => handleDocumentReview(doc.id, "Approved")}
                  className="text-sm px-3 py-2 border rounded"
                  data-testid="sks-approve-document"
                >
                  Onayla
                </button>
                <button
                  onClick={() => handleDocumentReview(doc.id, "Rejected")}
                  className="text-sm px-3 py-2 border rounded"
                  data-testid="sks-reject-document"
                >
                  Reddet
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
