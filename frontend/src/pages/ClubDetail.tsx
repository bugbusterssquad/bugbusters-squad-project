import { useEffect, useState } from "react";
import { Link, useParams } from "react-router";
import { api } from "../services/api";
import type { Announcement, Club, EventItem } from "../types";
import MembershipApply from "../components/MembershipApply";
import { getErrorMessage } from "../utils/error";

export default function ClubDetail() {
  const { id } = useParams();
  const clubId = Number(id);
  const [club, setClub] = useState<Club | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [events, setEvents] = useState<EventItem[]>([]);
  const [eventsError, setEventsError] = useState<string | null>(null);
  const [announcements, setAnnouncements] = useState<Announcement[]>([]);
  const [announcementsError, setAnnouncementsError] = useState<string | null>(null);

  useEffect(() => {
    if (!clubId) return;
    (async () => {
      try {
        setLoading(true);
        const data = await api.getClub(clubId);
        setClub(data);
      } catch (e) {
        setError(getErrorMessage(e, "Hata"));
      } finally {
        setLoading(false);
      }
    })();
  }, [clubId]);

  useEffect(() => {
    if (!clubId) return;
    (async () => {
      try {
        const data = await api.listEvents({ clubId, pageSize: 5 });
        setEvents(data.items);
      } catch (e) {
        setEventsError(getErrorMessage(e, "Etkinlikler yüklenemedi."));
      }
    })();
  }, [clubId]);

  useEffect(() => {
    if (!clubId) return;
    (async () => {
      try {
        const data = await api.getClubAnnouncements(clubId);
        setAnnouncements(data);
      } catch (e) {
        setAnnouncementsError(getErrorMessage(e, "Duyurular yüklenemedi."));
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
                <h4 className="font-semibold">Açıklama</h4>
                <p className="text-gray-700">{club.description ?? "Açıklama bilgisi bulunmuyor."}</p>
              </div>
              <div>
                <h4 className="font-semibold">Kategori</h4>
                <p className="text-gray-700">{club.category ?? "Kategori bilgisi bulunmuyor."}</p>
              </div>
              <div>
                <h4 className="font-semibold">İletişim</h4>
                <p className="text-gray-700">{club.contact ?? "İletişim bilgisi bulunmuyor."}</p>
              </div>
            </div>
          </div>

          <div className="bg-white p-6 rounded-md shadow">
            <h3 className="text-lg font-semibold">Aktif Etkinlikler</h3>
            {eventsError && <div className="text-red-500 mt-2">{eventsError}</div>}
            {events.length === 0 && !eventsError && (
              <p className="text-gray-600 mt-3">Yaklaşan etkinlik bulunmuyor.</p>
            )}
            <div className="mt-4 space-y-3">
              {events.map((event) => (
                <Link key={event.id} to={`/events/${event.id}`} className="block border rounded p-3 hover:shadow-sm">
                  <h4 className="font-semibold">{event.title}</h4>
                  <p className="text-sm text-gray-600 mt-1">
                    {new Date(event.startAt).toLocaleString("tr-TR")} · {event.location ?? "Konum belirtilmedi"}
                  </p>
                </Link>
              ))}
            </div>
          </div>

          <div className="bg-white p-6 rounded-md shadow">
            <h3 className="text-lg font-semibold">Duyurular</h3>
            {announcementsError && <div className="text-red-500 mt-2">{announcementsError}</div>}
            {announcements.length === 0 && !announcementsError && (
              <p className="text-gray-600 mt-3">Henüz duyuru yok.</p>
            )}
            <div className="mt-4 space-y-3">
              {announcements.map((announcement) => (
                <div key={announcement.id} className="border rounded p-3">
                  <h4 className="font-semibold">{announcement.title}</h4>
                  <p className="text-sm text-gray-600 mt-1">{announcement.content}</p>
                </div>
              ))}
            </div>
          </div>

          <MembershipApply clubId={club.id} />
        </div>
      )}
    </div>
  );
}
