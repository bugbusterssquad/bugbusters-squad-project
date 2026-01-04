import { useEffect, useState } from "react";
import { Link } from "react-router";
import { api } from "../services/api";
import type { ClubOption, EventItem } from "../types";
import { getErrorMessage } from "../utils/error";

export default function Events() {
  const [events, setEvents] = useState<EventItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [clubs, setClubs] = useState<ClubOption[]>([]);
  const [clubId, setClubId] = useState("");
  const [start, setStart] = useState("");
  const [end, setEnd] = useState("");
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const pageSize = 6;

  useEffect(() => {
    (async () => {
      try {
        setLoading(true);
        setError(null);
        const data = await api.listEvents({
          clubId: clubId ? Number(clubId) : undefined,
          start: start || undefined,
          end: end || undefined,
          page,
          pageSize
        });
        setEvents(data.items);
        setTotal(data.total);
      } catch (e) {
        setError(getErrorMessage(e));
      } finally {
        setLoading(false);
      }
    })();
  }, [clubId, start, end, page]);

  useEffect(() => {
    (async () => {
      try {
        const data = await api.listClubOptions();
        setClubs(data);
      } catch (e) {
        setError(getErrorMessage(e));
      }
    })();
  }, []);

  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  return (
    <div>
      <div className="mb-6">
        <h2 className="text-2xl font-bold">Etkinlikler</h2>
        <p className="text-gray-600 mt-1">Yaklaşan etkinlikleri keşfet.</p>
      </div>

      <div className="bg-white rounded-md shadow p-4 mb-6 grid gap-3 md:grid-cols-4">
        <select
          value={clubId}
          onChange={(e) => {
            setPage(1);
            setClubId(e.target.value);
          }}
          className="w-full p-2 border rounded"
          aria-label="Kulüp filtresi"
        >
          <option value="">Tüm kulüpler</option>
          {clubs.map((club) => (
            <option key={club.id} value={club.id}>{club.name}</option>
          ))}
        </select>
        <input
          type="date"
          value={start}
          onChange={(e) => {
            setPage(1);
            setStart(e.target.value);
          }}
          className="w-full p-2 border rounded"
          aria-label="Başlangıç tarihi"
        />
        <input
          type="date"
          value={end}
          onChange={(e) => {
            setPage(1);
            setEnd(e.target.value);
          }}
          className="w-full p-2 border rounded"
          aria-label="Bitiş tarihi"
        />
        <div className="flex items-center justify-between text-sm text-gray-600">
          <span>Toplam: {total}</span>
          <span>Sayfa: {page} / {totalPages}</span>
        </div>
      </div>

      {loading && <div>Yükleniyor...</div>}
      {error && <div className="text-red-500">{error}</div>}

      <div className="grid gap-4 grid-cols-1 md:grid-cols-2">
        {events.map((event) => (
          <Link
            key={event.id}
            to={`/events/${event.id}`}
            className="block bg-white shadow-sm rounded-md p-4 hover:shadow-md transition"
            data-event-title={event.title}
          >
            <div className="flex items-center justify-between gap-3">
              <h3 className="text-lg font-semibold">{event.title}</h3>
              <span className="text-xs px-2 py-1 bg-gray-100 rounded">{event.clubName}</span>
            </div>
            <p className="text-sm text-gray-500 mt-2 line-clamp-2">{event.description ?? "Detay için tıklayın"}</p>
            <div className="mt-3 text-sm text-gray-600 flex flex-wrap gap-3">
              <span>{new Date(event.startAt).toLocaleString("tr-TR")}</span>
              <span>{event.location ?? "Konum belirtilmedi"}</span>
            </div>
          </Link>
        ))}
      </div>

      <div className="mt-6 flex items-center justify-between">
        <button
          onClick={() => setPage((p) => Math.max(1, p - 1))}
          disabled={page <= 1}
          className="px-4 py-2 border rounded disabled:opacity-50"
        >
          Önceki
        </button>
        <button
          onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
          disabled={page >= totalPages}
          className="px-4 py-2 border rounded disabled:opacity-50"
        >
          Sonraki
        </button>
      </div>
    </div>
  );
}
