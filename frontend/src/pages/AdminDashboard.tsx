import React, { useEffect, useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router";
import { api } from "../services/api";
import { getUser } from "../services/auth";
import type { AdminEvent, Announcement } from "../types";
import { getErrorMessage } from "../utils/error";

export default function AdminDashboard() {
  const user = getUser();
  const userId = user?.id ?? null;
  const userRole = user?.role ?? null;
  const navigate = useNavigate();

  const documentRef = useRef<HTMLInputElement>(null);

  const [clubs, setClubs] = useState<{ id: number; name: string }[]>([]);
  const [selectedClubId, setSelectedClubId] = useState<number | null>(null);
  const [events, setEvents] = useState<AdminEvent[]>([]);
  const [applications, setApplications] = useState<
    {
      id: number;
      userId: number;
      userName: string;
      userEmail: string;
      status: string;
      note?: string | null;
      createdAt: string;
    }[]
  >([]);
  const [announcements, setAnnouncements] = useState<Announcement[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const [eventForm, setEventForm] = useState({
    title: "",
    description: "",
    location: "",
    startAt: "",
    endAt: "",
    capacity: "0",
  });

  const [announcementForm, setAnnouncementForm] = useState({
    title: "",
    content: "",
    status: "Published",
  });

  const [applicationNotes, setApplicationNotes] = useState<
    Record<number, string>
  >({});
  const [clubApplicationMessage, setClubApplicationMessage] = useState<
    string | null
  >(null);
  const [documentEventId, setDocumentEventId] = useState<string>("");
  const [documentFile, setDocumentFile] = useState<File | null>(null);
  const [documentMessage, setDocumentMessage] = useState<string | null>(null);

  useEffect(() => {
    if (!userId) {
      navigate("/login");
      return;
    }
    if (userRole !== "ClubAdmin" && userRole !== "SuperAdmin") {
      navigate("/");
      return;
    }

    (async () => {
      try {
        setLoading(true);
        const data = await api.listAdminClubs();
        setClubs(data);
        setSelectedClubId(data[0]?.id ?? null);
      } catch (e) {
        setError(getErrorMessage(e));
      } finally {
        setLoading(false);
      }
    })();
  }, [userId, userRole, navigate]);

  useEffect(() => {
    if (!selectedClubId) return;
    (async () => {
      try {
        setError(null);
        const [eventData, appData, announcementData] = await Promise.all([
          api.listAdminEvents(selectedClubId),
          api.listClubApplications(selectedClubId),
          api.listAdminAnnouncements(selectedClubId),
        ]);
        setEvents(eventData);
        setApplications(appData);
        setAnnouncements(announcementData);
      } catch (e) {
        setError(getErrorMessage(e));
      }
    })();
  }, [selectedClubId]);

  const selectedClub = useMemo(
    () => clubs.find((club) => club.id === selectedClubId),
    [clubs, selectedClubId]
  );

  async function handleCreateEvent(e: React.FormEvent) {
    e.preventDefault();
    if (!selectedClubId) return;

    try {
      await api.createEvent(selectedClubId, {
        title: eventForm.title,
        description: eventForm.description || null,
        location: eventForm.location || null,
        startAt: new Date(eventForm.startAt).toISOString(),
        endAt: new Date(eventForm.endAt).toISOString(),
        capacity: Number(eventForm.capacity),
      });
      const data = await api.listAdminEvents(selectedClubId);
      setEvents(data);
      setEventForm({
        title: "",
        description: "",
        location: "",
        startAt: "",
        endAt: "",
        capacity: "0",
      });
    } catch (e) {
      setError(getErrorMessage(e));
    }
  }

  async function handleUpdateEvent(event: AdminEvent, status: string) {
    try {
      await api.updateEvent(event.id, { ...event, status });
      const data = await api.listAdminEvents(event.clubId);
      setEvents(data);
    } catch (e) {
      setError(getErrorMessage(e));
    }
  }

  async function handleCreateAnnouncement(e: React.FormEvent) {
    e.preventDefault();
    if (!selectedClubId) return;

    try {
      await api.createAnnouncement(selectedClubId, announcementForm);
      const data = await api.listAdminAnnouncements(selectedClubId);
      setAnnouncements(data);
      setAnnouncementForm({ title: "", content: "", status: "Published" });
    } catch (e) {
      setError(getErrorMessage(e));
    }
  }

  async function handleToggleAnnouncement(
    announcement: Announcement,
    status: string
  ) {
    try {
      await api.updateAnnouncement(announcement.id, {
        title: announcement.title,
        content: announcement.content,
        status,
      });
      if (!selectedClubId) return;
      const data = await api.listAdminAnnouncements(selectedClubId);
      setAnnouncements(data);
    } catch (e) {
      setError(getErrorMessage(e));
    }
  }

  async function handleApplicationAction(
    id: number,
    status: "Approved" | "Rejected"
  ) {
    try {
      const note = applicationNotes[id];
      await api.updateClubApplication(id, status, note);
      if (!selectedClubId) return;
      const data = await api.listClubApplications(selectedClubId);
      setApplications(data);
    } catch (e) {
      setError(getErrorMessage(e));
    }
  }

  async function handleSubmitClubApplication() {
    if (!selectedClubId) return;
    try {
      setClubApplicationMessage(null);
      const res = await api.submitSksClubApplication(selectedClubId);
      setClubApplicationMessage(res.message);
    } catch (e) {
      setClubApplicationMessage(getErrorMessage(e));
    }
  }

  async function handleUploadDocument(e: React.FormEvent) {
    e.preventDefault();
    if (!documentEventId || !documentFile) {
      setDocumentMessage("Lütfen Belge Yükleyiniz!!");
      return;
    }
    try {
      setDocumentMessage(null);
      const res = await api.uploadEventDocument(
        Number(documentEventId),
        documentFile
      );
      setDocumentMessage(res.message);
      setDocumentFile(null);
    } catch (e) {
      setDocumentMessage(getErrorMessage(e));
    }
  }

  if (loading) return <div>Yükleniyor...</div>;

  return (
    <div className="max-w-5xl mx-auto space-y-8">
      <div className="bg-white p-6 rounded-md shadow">
        <h2 className="text-2xl font-bold">Kulüp Admin Paneli</h2>
        <p className="text-gray-600 mt-1">
          {selectedClub?.name ?? "Kulüp seç"}
        </p>
        {error && <div className="text-red-500 mt-2">{error}</div>}
        <div className="mt-4">
          <select
            value={selectedClubId ?? ""}
            onChange={(e) => setSelectedClubId(Number(e.target.value))}
            className="w-full md:w-1/2 p-2 border rounded"
            aria-label="Kulüp seçimi"
          >
            {clubs.map((club) => (
              <option key={club.id} value={club.id}>
                {club.name}
              </option>
            ))}
          </select>
        </div>
        <div className="mt-4">
          <button
            onClick={handleSubmitClubApplication}
            className="px-4 py-2 border rounded"
            data-testid="admin-sks-submit"
          >
            SKS Başvurusu Gönder
          </button>
          {clubApplicationMessage && (
            <p className="text-sm text-gray-600 mt-2">
              {clubApplicationMessage}
            </p>
          )}
        </div>
      </div>

      <div className="bg-white p-6 rounded-md shadow">
        <h3 className="text-lg font-semibold">Etkinlik Oluştur</h3>
        <form onSubmit={handleCreateEvent} className="mt-4 space-y-3">
          <input
            value={eventForm.title}
            onChange={(e) =>
              setEventForm((prev) => ({ ...prev, title: e.target.value }))
            }
            className="w-full p-2 border rounded"
            placeholder="Etkinlik başlığı"
            aria-label="Etkinlik başlığı"
            data-testid="admin-event-title"
            required
          />
          <textarea
            value={eventForm.description}
            onChange={(e) =>
              setEventForm((prev) => ({ ...prev, description: e.target.value }))
            }
            className="w-full p-2 border rounded"
            rows={3}
            placeholder="Etkinlik açıklaması"
            aria-label="Etkinlik açıklaması"
          />
          <input
            value={eventForm.location}
            onChange={(e) =>
              setEventForm((prev) => ({ ...prev, location: e.target.value }))
            }
            className="w-full p-2 border rounded"
            placeholder="Konum"
            aria-label="Etkinlik konumu"
          />
          <div className="grid gap-5 grid-cols-3">
            <div className="flex flex-row w-full items-center">
              <label htmlFor="event-start" className="w-fit mr-2">
                Başlangıç :
              </label>
              <input
                id="event-start"
                type="datetime-local"
                value={eventForm.startAt}
                onChange={(e) =>
                  setEventForm((prev) => ({ ...prev, startAt: e.target.value }))
                }
                className="p-2 border rounded flex-1"
                aria-label="Etkinlik başlangıç"
                data-testid="admin-event-start"
                required
              />
            </div>

            <div className="flex flex-row w-full items-center">
              <label htmlFor="event-end" className="w-fit mr-2">
                Bitiş :
              </label>
              <input
                id="event-end"
                type="datetime-local"
                value={eventForm.endAt}
                onChange={(e) =>
                  setEventForm((prev) => ({ ...prev, endAt: e.target.value }))
                }
                className="flex-1 p-2 border rounded"
                aria-label="Etkinlik bitiş"
                data-testid="admin-event-end"
                required
              />
            </div>
            <div className="flex flex-row w-full items-center">
              <label htmlFor="capacity" className="w-fit mr-2">
                Kapasite :
              </label>
              <input
                id="capacity"
                type="number"
                min="0"
                value={eventForm.capacity}
                onChange={(e) =>
                  setEventForm((prev) => ({
                    ...prev,
                    capacity: e.target.value,
                  }))
                }
                className="flex-1 p-2 border rounded"
                placeholder="Kapasite"
                aria-label="Etkinlik kapasitesi"
                data-testid="admin-event-capacity"
              />
            </div>
          </div>

          <button
            className="px-4 py-2 bg-accent text-black border border-slate-950 rounded"
            data-testid="admin-event-create"
          >
            Oluştur
          </button>
        </form>
      </div>

      <div className="bg-white p-6 rounded-md shadow">
        <h3 className="text-lg font-semibold">Etkinlikler</h3>
        <div className="mt-4 space-y-3">
          {events.length === 0 && (
            <p className="text-gray-600">Henüz etkinlik yok.</p>
          )}
          {events.map((event) => (
            <div
              key={event.id}
              className="border rounded p-4 flex flex-col gap-3 md:flex-row md:items-center md:justify-between"
              data-testid="admin-event-card"
              data-event-title={event.title}
            >
              <div>
                <p className="font-semibold">{event.title}</p>
                <p className="text-sm text-gray-600">
                  {new Date(event.startAt).toLocaleString("tr-TR")} ·{" "}
                  {event.status}
                </p>
              </div>
              <div className="flex items-center gap-2">
                {event.status !== "Published" && (
                  <button
                    onClick={() => handleUpdateEvent(event, "Published")}
                    className="text-sm px-3 py-2 border rounded"
                    data-testid="admin-event-publish"
                  >
                    Yayınla
                  </button>
                )}
                {event.status !== "Cancelled" && (
                  <button
                    onClick={() => handleUpdateEvent(event, "Cancelled")}
                    className="text-sm px-3 py-2 border rounded"
                    data-testid="admin-event-cancel"
                  >
                    İptal Et
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
        <form onSubmit={handleUploadDocument} className="mt-6 space-y-3">
          <h4 className="font-semibold">Etkinlik Belgesi Yükle</h4>
          <div className="flex flex-row gap-5">
            <select
              value={documentEventId}
              onChange={(e) => setDocumentEventId(e.target.value)}
              className="flex-1 p-2 border rounded"
              aria-label="Belge etkinlik seçimi"
              data-testid="admin-doc-event"
              required
            >
              <option value="">Etkinlik seç</option>
              {events.map((event) => (
                <option key={event.id} value={event.id}>
                  {event.title}
                </option>
              ))}
            </select>

            <button
              disabled={!documentEventId}
              onClick={() => documentRef.current?.click()}
              className="flex-1 border border-slate-950 text-slate-950 rounded disabled:opacity-40 disabled:cursor-not-allowed"
            >
              {documentFile ? documentFile.name : "Belge Yükle"}
            </button>
            <input
              ref={documentRef}
              hidden
              type="file"
              onChange={(e) => setDocumentFile(e.target.files?.[0] ?? null)}
              className="flex-1 border border-slate-950 rounded text-center"
              accept=".pdf,image/png,image/jpeg"
              aria-label="Belge dosyası"
              data-testid="admin-doc-file"
            />
          </div>

          <button
            className="px-4 py-2 border rounded"
            data-testid="admin-doc-submit"
          >
            Belge Gönder
          </button>
          {documentMessage && (
            <p className="text-sm text-gray-600">{documentMessage}</p>
          )}
        </form>
      </div>

      <div className="bg-white p-6 rounded-md shadow">
        <h3 className="text-lg font-semibold">Kulüp Başvuruları</h3>
        <div className="mt-4 space-y-3">
          {applications.length === 0 && (
            <p className="text-gray-600">Başvuru yok.</p>
          )}
          {applications.map((app) => (
            <div key={app.id} className="border rounded p-4 space-y-2">
              <div className="flex flex-col gap-1">
                <p className="font-semibold">{app.userName}</p>
                <p className="text-sm text-gray-600">{app.userEmail}</p>
                <p className="text-sm">Durum: {app.status}</p>
              </div>
              <input
                value={applicationNotes[app.id] ?? ""}
                onChange={(e) =>
                  setApplicationNotes((prev) => ({
                    ...prev,
                    [app.id]: e.target.value,
                  }))
                }
                className="w-full p-2 border rounded"
                placeholder="Not ekle"
                aria-label="Kulüp başvurusu notu"
              />
              <div className="flex items-center gap-2">
                <button
                  onClick={() => handleApplicationAction(app.id, "Approved")}
                  className="text-sm px-3 py-2 border rounded"
                >
                  Onayla
                </button>
                <button
                  onClick={() => handleApplicationAction(app.id, "Rejected")}
                  className="text-sm px-3 py-2 border rounded"
                >
                  Reddet
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>

      <div className="bg-white p-6 rounded-md shadow">
        <h3 className="text-lg font-semibold">Duyurular</h3>
        <form onSubmit={handleCreateAnnouncement} className="mt-4 space-y-3">
          <input
            value={announcementForm.title}
            onChange={(e) =>
              setAnnouncementForm((prev) => ({
                ...prev,
                title: e.target.value,
              }))
            }
            className="w-full p-2 border rounded"
            placeholder="Başlık"
            aria-label="Duyuru başlığı"
            required
          />
          <textarea
            value={announcementForm.content}
            onChange={(e) =>
              setAnnouncementForm((prev) => ({
                ...prev,
                content: e.target.value,
              }))
            }
            className="w-full p-2 border rounded"
            rows={3}
            placeholder="İçerik"
            aria-label="Duyuru içeriği"
            required
          />
          <select
            value={announcementForm.status}
            onChange={(e) =>
              setAnnouncementForm((prev) => ({
                ...prev,
                status: e.target.value,
              }))
            }
            className="w-full p-2 border rounded"
            aria-label="Duyuru durumu"
          >
            <option value="Published">Yayınla</option>
            <option value="Hidden">Gizle</option>
          </select>
          <button className="px-4 py-2 bg-accent text-slate-950 border border-slate-950 rounded">
            Duyuru Oluştur
          </button>
        </form>

        <div className="mt-4 space-y-3">
          {announcements.length === 0 && (
            <p className="text-gray-600">Henüz duyuru yok.</p>
          )}
          {announcements.map((announcement) => (
            <div
              key={announcement.id}
              className="border rounded p-4 flex flex-col gap-2 md:flex-row md:items-center md:justify-between"
            >
              <div>
                <p className="font-semibold">{announcement.title}</p>
                <p className="text-sm text-gray-600">{announcement.status}</p>
              </div>
              <div className="flex items-center gap-2">
                {announcement.status !== "Published" && (
                  <button
                    onClick={() =>
                      handleToggleAnnouncement(announcement, "Published")
                    }
                    className="text-sm px-3 py-2 border rounded"
                  >
                    Yayınla
                  </button>
                )}
                {announcement.status !== "Hidden" && (
                  <button
                    onClick={() =>
                      handleToggleAnnouncement(announcement, "Hidden")
                    }
                    className="text-sm px-3 py-2 border rounded"
                  >
                    Gizle
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
