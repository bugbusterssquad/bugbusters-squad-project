import React, { useEffect, useState } from "react";
import { api } from "../services/api";
import { useNavigate } from "react-router";
import { getUser, updateStoredUser } from "../services/auth";
import type { ClubApplication, Profile, NotificationItem } from "../types";
import { getErrorMessage } from "../utils/error";

export default function Profile() {
  const user = getUser();
  const userId = user?.id ?? null;
  const navigate = useNavigate();
  const [profile, setProfile] = useState<Profile | null>(null);
  const [form, setForm] = useState({
    name: "",
    faculty: "",
    department: "",
    bio: "",
    avatarUrl: "",
  });
  const [applications, setApplications] = useState<ClubApplication[]>([]);
  const [notifications, setNotifications] = useState<NotificationItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [err, setErr] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState<string | null>(null);

  useEffect(() => {
    if (!userId) {
      navigate("/login");
      return;
    }
    (async () => {
      try {
        setLoading(true);
        const [profileData, appsData, notificationsData] = await Promise.all([
          api.getProfile(),
          api.getMyApplications(),
          api.listNotifications(),
        ]);
        setProfile(profileData);
        setApplications(appsData);
        setNotifications(notificationsData);
        setForm({
          name: profileData.name ?? "",
          faculty: profileData.faculty ?? "",
          department: profileData.department ?? "",
          bio: profileData.bio ?? "",
          avatarUrl: profileData.avatarUrl ?? "",
        });
      } catch (e) {
        setErr(getErrorMessage(e));
      } finally {
        setLoading(false);
      }
    })();
  }, [userId, navigate]);

  if (!userId) return null;

  async function handleSave(e: React.FormEvent) {
    e.preventDefault();
    try {
      setSaving(true);
      setMessage(null);
      const updated = await api.updateProfile({
        name: form.name.trim(),
        faculty: form.faculty.trim() || null,
        department: form.department.trim() || null,
        bio: form.bio.trim() || null,
        avatarUrl: form.avatarUrl.trim() || null,
      });
      setProfile(updated);
      updateStoredUser({ name: updated.name });
      setMessage("Profil güncellendi.");
    } catch (e) {
      setMessage(getErrorMessage(e, "Profil güncellenemedi."));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div className="bg-white p-6 rounded-md shadow">
        <h2 className="text-xl font-semibold">Profil</h2>
        {profile && (
          <p className="mt-2 text-sm text-gray-600">
            <strong>Email:</strong> {profile.email} · <strong>Rol:</strong>{" "}
            {profile.role}
          </p>
        )}
        <form onSubmit={handleSave} className="mt-4 space-y-3">
          <input
            value={form.name}
            onChange={(e) =>
              setForm((prev) => ({ ...prev, name: e.target.value }))
            }
            className="w-full p-2 border rounded"
            placeholder="Ad Soyad"
            aria-label="Ad Soyad"
            required
          />
          <div className="grid gap-3 md:grid-cols-2">
            <input
              value={form.faculty}
              onChange={(e) =>
                setForm((prev) => ({ ...prev, faculty: e.target.value }))
              }
              className="w-full p-2 border rounded"
              placeholder="Fakülte"
              aria-label="Fakülte"
            />
            <input
              value={form.department}
              onChange={(e) =>
                setForm((prev) => ({ ...prev, department: e.target.value }))
              }
              className="w-full p-2 border rounded"
              placeholder="Bölüm"
              aria-label="Bölüm"
            />
          </div>
          <textarea
            value={form.bio}
            onChange={(e) =>
              setForm((prev) => ({ ...prev, bio: e.target.value }))
            }
            className="w-full p-2 border rounded"
            rows={4}
            placeholder="Kısa biyografi"
            aria-label="Biyografi"
          />
          <input
            value={form.avatarUrl}
            onChange={(e) =>
              setForm((prev) => ({ ...prev, avatarUrl: e.target.value }))
            }
            className="w-full p-2 border rounded"
            placeholder="Avatar URL (opsiyonel)"
            aria-label="Avatar URL"
          />
          <button
            disabled={saving}
            className="px-4 py-2 bg-accent text-slate-950 bg-slate-100 rounded"
          >
            {saving ? "Kaydediliyor..." : "Profili Güncelle"}
          </button>
          {message && <p className="text-sm text-gray-600">{message}</p>}
        </form>
      </div>

      <div className="bg-white p-6 rounded-md shadow">
        <h3 className="text-lg font-semibold">Kulüp Başvurularım</h3>
        <p className="text-gray-600 mt-2">
          Başvurularının durumu ve onaylanan kulüplerin QR kartları burada
          görünür.
        </p>

        {loading && <div>Yükleniyor...</div>}
        {err && <div className="text-red-500">{err}</div>}

        {!loading && applications.length === 0 && (
          <div className="mt-4">
            <p className="text-gray-700">Henüz bir başvurun bulunmuyor.</p>
            <button
              onClick={() => navigate("/")}
              className="mt-3 px-4 py-2 bg-accent text-white rounded"
            >
              Kulüpleri Gör
            </button>
          </div>
        )}

        {applications.length > 0 && (
          <div className="mt-4 space-y-4">
            {applications.map((app) => (
              <div
                key={app.id}
                className="border rounded-md p-4 flex flex-col gap-3 md:flex-row md:items-center md:justify-between"
              >
                <div>
                  <p className="font-semibold">{app.clubName}</p>
                  <p className="text-sm text-gray-600">
                    Durum: <span className="capitalize">{app.status}</span>
                  </p>
                  {app.note && (
                    <p className="text-sm text-gray-500 mt-1">{app.note}</p>
                  )}
                </div>
                {app.qrCodeBase64 && (
                  <img
                    src={`data:image/png;base64,${app.qrCodeBase64}`}
                    alt="Dijital Üye Kartı"
                    className="w-28 h-28 object-contain border rounded"
                  />
                )}
              </div>
            ))}
          </div>
        )}
      </div>

      <div className="bg-white p-6 rounded-md shadow">
        <h3 className="text-lg font-semibold">Bildirimler</h3>
        {notifications.length === 0 && (
          <p className="text-gray-600 mt-2">Henüz bildirim yok.</p>
        )}
        <div className="mt-3 space-y-3" data-testid="notifications-list">
          {notifications.map((notification) => (
            <div
              key={notification.id}
              className="border rounded p-3 flex flex-col gap-2 md:flex-row md:items-center md:justify-between"
            >
              <div>
                <p className="font-medium">{notification.type}</p>
                <p className="text-sm text-gray-600">
                  {new Date(notification.createdAt).toLocaleString("tr-TR")}
                </p>
              </div>
              <div className="flex items-center gap-3">
                {notification.readAt ? (
                  <span className="text-xs px-2 py-1 bg-gray-100 rounded">
                    Okundu
                  </span>
                ) : (
                  <button
                    className="text-sm px-3 py-1 border rounded"
                    onClick={async () => {
                      await api.markNotificationRead(notification.id);
                      setNotifications((prev) =>
                        prev.map((item) =>
                          item.id === notification.id
                            ? { ...item, readAt: new Date().toISOString() }
                            : item
                        )
                      );
                    }}
                  >
                    Okundu işaretle
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
