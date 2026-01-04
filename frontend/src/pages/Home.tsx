import { useEffect, useMemo, useState } from "react";
import ClubCard from "../components/ClubCard";
import { api } from "../services/api";
import type { Club, EventItem } from "../types";
import { getErrorMessage } from "../utils/error";
import { Link } from "react-router";

export default function Home() {
  const [clubs, setClubs] = useState<Club[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [events, setEvents] = useState<EventItem[]>([]);
  const [eventsLoading, setEventsLoading] = useState(true);
  const [eventsError, setEventsError] = useState<string | null>(null);
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [category, setCategory] = useState("");
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const pageSize = 9;

  const categories = useMemo(() => {
    const values = clubs
      .map((club) => club.category)
      .filter((value): value is string => Boolean(value));
    return Array.from(new Set(values)).sort();
  }, [clubs]);

  useEffect(() => {
    const timer = setTimeout(() => {
      setPage(1);
      setSearch(searchInput.trim());
    }, 300);
    return () => clearTimeout(timer);
  }, [searchInput]);

  useEffect(() => {
    (async () => {
      try {
        setLoading(true);
        setError(null);
        const data = await api.listClubs({ search, category, page, pageSize });
        setClubs(data.items);
        setTotal(data.total);
      } catch (e) {
        setError(getErrorMessage(e));
      } finally {
        setLoading(false);
      }
    })();
  }, [search, category, page]);

  useEffect(() => {
    (async () => {
      try {
        setEventsLoading(true);
        setEventsError(null);
        const data = await api.listEvents({ pageSize: 4 });
        setEvents(data.items);
      } catch (e) {
        setEventsError(getErrorMessage(e));
      } finally {
        setEventsLoading(false);
      }
    })();
  }, []);

  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  return (
    <div>
      <div className="mb-6">
        <h2 className="text-2xl font-bold">Kulüpler</h2>
        <p className="text-gray-600 mt-1">İlgini çeken kulübe tıklayarak detaylarını inceleyebilirsin.</p>
      </div>

      <div className="bg-white rounded-md shadow p-4 mb-6 grid gap-3 md:grid-cols-3">
        <input
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
          className="w-full p-2 border rounded"
          placeholder="Kulüp ara..."
          aria-label="Kulüp arama"
        />
        <select
          value={category}
          onChange={(e) => {
            setPage(1);
            setCategory(e.target.value);
          }}
          className="w-full p-2 border rounded"
          aria-label="Kategori filtresi"
        >
          <option value="">Tüm kategoriler</option>
          {categories.map((cat) => (
            <option key={cat} value={cat}>{cat}</option>
          ))}
        </select>
        <div className="flex items-center justify-between text-sm text-gray-600">
          <span>Toplam: {total}</span>
          <span>Sayfa: {page} / {totalPages}</span>
        </div>
      </div>

      {loading && <div>Yükleniyor...</div>}
      {error && <div className="text-red-500">{error}</div>}

      <div className="grid gap-4 grid-cols-1 sm:grid-cols-2 lg:grid-cols-3">
        {clubs.map((c) => <ClubCard key={c.id} club={c} />)}
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

      <div className="mt-12 mb-6 flex items-center justify-between">
        <h2 className="text-2xl font-bold">Yaklaşan Etkinlikler</h2>
        <Link to="/events" className="text-sm text-accent">Tümünü gör</Link>
      </div>

      {eventsLoading && <div>Etkinlikler yükleniyor...</div>}
      {eventsError && <div className="text-red-500">{eventsError}</div>}
      <div className="grid gap-4 grid-cols-1 md:grid-cols-2">
        {events.map((event) => (
          <Link
            key={event.id}
            to={`/events/${event.id}`}
            className="block bg-white shadow-sm rounded-md p-4 hover:shadow-md transition"
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
    </div>
  );
}
