import React, { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router";
import { api } from "../services/api";
import type { CommentItem, EventItem, ReactionSummary } from "../types";
import { getErrorMessage } from "../utils/error";
import { getUser } from "../services/auth";

export default function EventDetail() {
  const { id } = useParams();
  const eventId = Number(id);
  const [event, setEvent] = useState<EventItem | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [registrationStatus, setRegistrationStatus] = useState<string | null>(null);
  const [actionMessage, setActionMessage] = useState<string | null>(null);
  const [actionLoading, setActionLoading] = useState(false);
  const [comments, setComments] = useState<CommentItem[]>([]);
  const [commentBody, setCommentBody] = useState("");
  const [commentMessage, setCommentMessage] = useState<string | null>(null);
  const [reaction, setReaction] = useState<ReactionSummary | null>(null);
  const [reactionLoading, setReactionLoading] = useState(false);
  const user = getUser();
  const userId = user?.id ?? null;
  const navigate = useNavigate();
  const isAdmin = user?.role === "ClubAdmin" || user?.role === "SuperAdmin";

  useEffect(() => {
    if (!eventId) return;
    (async () => {
      try {
        setLoading(true);
        const data = await api.getEvent(eventId);
        setEvent(data);
      } catch (e) {
        setError(getErrorMessage(e, "Hata"));
      } finally {
        setLoading(false);
      }
    })();
  }, [eventId]);

  useEffect(() => {
    if (!eventId) return;
    (async () => {
      try {
        const data = await api.listEventComments(eventId);
        setComments(data);
      } catch (e) {
        setCommentMessage(getErrorMessage(e, "Yorumlar yüklenemedi."));
      }
    })();
  }, [eventId]);

  useEffect(() => {
    if (!eventId) return;
    (async () => {
      try {
        const data = await api.getEventReactions(eventId);
        setReaction(data);
      } catch (e) {
        setCommentMessage(getErrorMessage(e, "Tepki bilgisi yüklenemedi."));
      }
    })();
  }, [eventId]);

  useEffect(() => {
    if (!eventId || !userId) return;
    (async () => {
      try {
        const reg = await api.getMyRegistration(eventId);
        setRegistrationStatus(reg.status);
      } catch (e) {
        const message = getErrorMessage(e, "Not Found");
        if (message === "Not Found") {
          setRegistrationStatus(null);
          return;
        }
      }
    })();
  }, [eventId, userId]);

  async function handleRegister() {
    if (!user) {
      navigate("/login");
      return;
    }
    try {
      setActionLoading(true);
      setActionMessage(null);
      const res = await api.registerEvent(eventId);
      setRegistrationStatus(res.status);
      setActionMessage(res.message);
    } catch (e) {
      setActionMessage(getErrorMessage(e, "Kayıt başarısız."));
    } finally {
      setActionLoading(false);
    }
  }

  async function handleCancel() {
    try {
      setActionLoading(true);
      setActionMessage(null);
      const res = await api.cancelRegistration(eventId);
      setRegistrationStatus("Cancelled");
      setActionMessage(res.message);
    } catch (e) {
      setActionMessage(getErrorMessage(e, "İptal başarısız."));
    } finally {
      setActionLoading(false);
    }
  }

  async function handleCommentSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!user) {
      navigate("/login");
      return;
    }
    try {
      setCommentMessage(null);
      await api.createEventComment(eventId, commentBody);
      setCommentBody("");
      const data = await api.listEventComments(eventId);
      setComments(data);
    } catch (e) {
      setCommentMessage(getErrorMessage(e, "Yorum gönderilemedi."));
    }
  }

  async function handleToggleReaction() {
    if (!user) {
      navigate("/login");
      return;
    }
    try {
      setReactionLoading(true);
      const data = await api.toggleEventReaction(eventId);
      setReaction(data);
    } catch (e) {
      setCommentMessage(getErrorMessage(e, "Tepki güncellenemedi."));
    } finally {
      setReactionLoading(false);
    }
  }

  async function handleDeleteComment(commentId: number) {
    try {
      await api.deleteComment(commentId);
      const data = await api.listEventComments(eventId);
      setComments(data);
    } catch (e) {
      setCommentMessage(getErrorMessage(e, "Yorum silinemedi."));
    }
  }

  async function handleHideComment(commentId: number) {
    try {
      await api.hideComment(commentId);
      const data = await api.listEventComments(eventId);
      setComments(data);
    } catch (e) {
      setCommentMessage(getErrorMessage(e, "Yorum gizlenemedi."));
    }
  }

  if (!eventId) return <div>Geçersiz etkinlik.</div>;

  return (
    <div>
      {loading && <div>Yükleniyor...</div>}
      {error && <div className="text-red-500">{error}</div>}
      {event && (
        <div className="space-y-6">
          <div className="bg-white p-6 rounded-md shadow">
            <div className="flex items-center justify-between gap-3">
              <h2 className="text-2xl font-bold">{event.title}</h2>
              <span className="text-sm px-2 py-1 bg-gray-100 rounded">{event.clubName}</span>
            </div>
            <p className="mt-3 text-gray-700">{event.description ?? "Açıklama bulunmuyor."}</p>
            <div className="mt-4 space-y-2 text-gray-600">
              <p><strong>Tarih:</strong> {new Date(event.startAt).toLocaleString("tr-TR")}</p>
              <p><strong>Süre:</strong> {new Date(event.endAt).toLocaleString("tr-TR")}</p>
              <p><strong>Konum:</strong> {event.location ?? "Konum belirtilmedi"}</p>
              <p><strong>Kapasite:</strong> {event.capacity}</p>
            </div>
            <div className="mt-4 flex flex-wrap items-center gap-3">
              {registrationStatus ? (
                <>
                  <span className="text-sm px-3 py-1 bg-gray-100 rounded">
                    Durum: {registrationStatus}
                  </span>
                  {registrationStatus !== "Cancelled" && (
                    <button
                      onClick={handleCancel}
                      disabled={actionLoading}
                      className="px-4 py-2 border rounded"
                    >
                      {actionLoading ? "İptal ediliyor..." : "Kaydı İptal Et"}
                    </button>
                  )}
                </>
              ) : (
                <button
                  onClick={handleRegister}
                  disabled={actionLoading}
                  className="px-4 py-2 bg-accent text-white rounded"
                  data-testid="event-register"
                >
                  {actionLoading ? "Kaydediliyor..." : "Etkinliğe Kayıt Ol"}
                </button>
              )}
              <button
                onClick={handleToggleReaction}
                disabled={reactionLoading}
                className="px-4 py-2 border rounded"
              >
                {reaction?.liked ? "Beğenildi" : "Beğen"} ({reaction?.total ?? 0})
              </button>
            </div>
            {actionMessage && <p className="text-sm text-gray-600 mt-2">{actionMessage}</p>}
            <Link to={`/clubs/${event.clubId}`} className="inline-block mt-4 text-accent">
              Kulüp detayına git
            </Link>
          </div>

          <div className="bg-white p-6 rounded-md shadow">
            <h3 className="text-lg font-semibold">Yorumlar</h3>
            <form onSubmit={handleCommentSubmit} className="mt-4 space-y-3">
              <textarea
                value={commentBody}
                onChange={(e) => setCommentBody(e.target.value)}
                className="w-full p-2 border rounded"
                rows={3}
                placeholder="Yorum yaz..."
                aria-label="Yorum"
                required
              />
              <button className="px-4 py-2 bg-accent text-white rounded">Yorum Gönder</button>
            </form>
            {commentMessage && <p className="text-sm text-gray-600 mt-2">{commentMessage}</p>}
            <div className="mt-4 space-y-3">
              {comments.length === 0 && <p className="text-gray-600">Henüz yorum yok.</p>}
              {comments.map((comment) => (
                <div key={comment.id} className="border rounded p-3">
                  <div className="flex items-center justify-between">
                    <p className="font-semibold">{comment.userName}</p>
                    <p className="text-xs text-gray-500">{new Date(comment.createdAt).toLocaleString("tr-TR")}</p>
                  </div>
                  <p className="text-sm text-gray-700 mt-2">{comment.body}</p>
                  <div className="mt-3 flex items-center gap-2">
                    {user?.id === comment.userId && (
                      <button onClick={() => handleDeleteComment(comment.id)} className="text-xs px-2 py-1 border rounded">
                        Sil
                      </button>
                    )}
                    {isAdmin && (
                      <button onClick={() => handleHideComment(comment.id)} className="text-xs px-2 py-1 border rounded">
                        Gizle
                      </button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
