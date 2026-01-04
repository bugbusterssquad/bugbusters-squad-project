import type {
  Club,
  EventItem,
  ClubApplication,
  Profile,
  ClubOption,
  Announcement,
  AdminEvent,
  NotificationItem,
  EventRegistration,
  AdminRegistration,
  SksClubApplication,
  SksEventDocument,
  CommentItem,
  ReactionSummary,
  AdminStats
} from "../types";
import { getToken } from "./auth";

const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5084";

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = getToken();
  const res = await fetch(`${API_URL}${path}`, {
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(options.headers || {})
    },
    ...options
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || res.statusText);
  }
  return (await res.json()) as T;
}

async function requestWithTotal<T>(path: string, options: RequestInit = {}) {
  const token = getToken();
  const res = await fetch(`${API_URL}${path}`, {
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(options.headers || {})
    },
    ...options
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || res.statusText);
  }

  const total = Number(res.headers.get("X-Total-Count") ?? "0");
  const items = (await res.json()) as T;
  return { items, total };
}

async function uploadForm<T>(path: string, formData: FormData): Promise<T> {
  const token = getToken();
  const res = await fetch(`${API_URL}${path}`, {
    method: "POST",
    headers: {
      ...(token ? { Authorization: `Bearer ${token}` } : {})
    },
    body: formData
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || res.statusText);
  }

  return (await res.json()) as T;
}

export const api = {
  listClubs: (options?: { search?: string; category?: string; page?: number; pageSize?: number }) => {
    const searchParams = new URLSearchParams();
    if (options?.search) searchParams.set("search", options.search);
    if (options?.category) searchParams.set("category", options.category);
    if (options?.page) searchParams.set("page", String(options.page));
    if (options?.pageSize) searchParams.set("pageSize", String(options.pageSize));
    const qs = searchParams.toString();
    return requestWithTotal<Club[]>(`/api/clubs${qs ? `?${qs}` : ""}`);
  },
  listClubOptions: () => request<ClubOption[]>("/api/clubs/options"),
  getClub: (id: number) => request<Club>(`/api/clubs/${id}`),
  listEvents: (options?: { clubId?: number; start?: string; end?: string; page?: number; pageSize?: number }) => {
    const searchParams = new URLSearchParams();
    if (options?.clubId) searchParams.set("clubId", String(options.clubId));
    if (options?.start) searchParams.set("start", options.start);
    if (options?.end) searchParams.set("end", options.end);
    if (options?.page) searchParams.set("page", String(options.page));
    if (options?.pageSize) searchParams.set("pageSize", String(options.pageSize));
    const qs = searchParams.toString();
    return requestWithTotal<EventItem[]>(`/api/events${qs ? `?${qs}` : ""}`);
  },
  getEvent: (id: number) => request<EventItem>(`/api/events/${id}`),
  register: (name: string, email: string, password: string) =>
    request<{ message: string }>("/api/auth/register", {
      method: "POST",
      body: JSON.stringify({ name, email, password })
    }),
  login: (email: string, password: string) =>
    request<{ token: string; user: { id: number; name: string; email: string; role: string } }>(
      "/api/auth/login",
      {
        method: "POST",
        body: JSON.stringify({ email, password })
      }
    ),
  logout: () =>
    request<{ message: string }>("/api/auth/logout", {
      method: "POST"
    }),
  me: () => request<{ id: number; name: string; email: string; role: string }>("/api/auth/me"),
  getProfile: () => request<Profile>("/api/profile/me"),
  updateProfile: (payload: Partial<Profile>) =>
    request<Profile>("/api/profile/me", {
      method: "PATCH",
      body: JSON.stringify(payload)
    }),
  listNotifications: () => request<NotificationItem[]>("/api/notifications"),
  markNotificationRead: (id: number) =>
    request<{ message: string }>(`/api/notifications/${id}/read`, {
      method: "PATCH"
    }),
  applyMembership: (clubId: number, note?: string) =>
    request<{ message: string }>("/api/clubs/" + clubId + "/applications", {
      method: "POST",
      body: JSON.stringify({ note })
    }),
  getMyApplications: () => request<ClubApplication[]>("/api/club-applications/me"),
  getClubAnnouncements: (clubId: number) => request<Announcement[]>(`/api/clubs/${clubId}/announcements`),
  listAdminClubs: () => request<{ id: number; name: string }[]>("/api/admin/clubs/mine"),
  listAdminEvents: (clubId: number) => request<AdminEvent[]>(`/api/admin/clubs/${clubId}/events`),
  createEvent: (clubId: number, payload: Omit<AdminEvent, "id" | "clubId" | "status">) =>
    request<{ message: string; id: number }>(`/api/clubs/${clubId}/events`, {
      method: "POST",
      body: JSON.stringify(payload)
    }),
  updateEvent: (eventId: number, payload: AdminEvent) =>
    request<{ message: string }>(`/api/events/${eventId}`, {
      method: "PATCH",
      body: JSON.stringify(payload)
    }),
  listClubApplications: (clubId: number) =>
    request<{ id: number; userId: number; userName: string; userEmail: string; status: string; note?: string | null; createdAt: string }[]>(
      `/api/clubs/${clubId}/applications`
    ),
  updateClubApplication: (id: number, status: string, note?: string) =>
    request<{ message: string }>(`/api/club-applications/${id}`, {
      method: "PATCH",
      body: JSON.stringify({ status, note })
    }),
  listAdminAnnouncements: (clubId: number) => request<Announcement[]>(`/api/admin/clubs/${clubId}/announcements`),
  createAnnouncement: (clubId: number, payload: { title: string; content: string; status: string }) =>
    request<{ message: string }>(`/api/clubs/${clubId}/announcements`, {
      method: "POST",
      body: JSON.stringify(payload)
    }),
  updateAnnouncement: (id: number, payload: { title: string; content: string; status: string }) =>
    request<{ message: string }>(`/api/announcements/${id}`, {
      method: "PATCH",
      body: JSON.stringify(payload)
    }),
  registerEvent: (eventId: number) =>
    request<{ message: string; status: string }>(`/api/events/${eventId}/registrations`, {
      method: "POST"
    }),
  getMyRegistration: (eventId: number) =>
    request<EventRegistration>(`/api/events/${eventId}/registrations/me`),
  cancelRegistration: (eventId: number) =>
    request<{ message: string }>(`/api/events/${eventId}/registrations/me`, {
      method: "DELETE"
    }),
  listEventRegistrations: (eventId: number) =>
    request<AdminRegistration[]>(`/api/events/${eventId}/registrations`),
  listEventComments: (eventId: number) => request<CommentItem[]>(`/api/events/${eventId}/comments`),
  createEventComment: (eventId: number, body: string) =>
    request<{ message: string }>(`/api/events/${eventId}/comments`, {
      method: "POST",
      body: JSON.stringify({ body })
    }),
  hideComment: (commentId: number) =>
    request<{ message: string }>(`/api/comments/${commentId}/hide`, {
      method: "PATCH"
    }),
  deleteComment: (commentId: number) =>
    request<{ message: string }>(`/api/comments/${commentId}`, {
      method: "DELETE"
    }),
  getEventReactions: (eventId: number) => request<ReactionSummary>(`/api/events/${eventId}/reactions`),
  toggleEventReaction: (eventId: number) =>
    request<ReactionSummary>(`/api/events/${eventId}/reactions`, { method: "POST" }),
  submitSksClubApplication: (clubId: number) =>
    request<{ message: string }>(`/api/sks/club-applications`, {
      method: "POST",
      body: JSON.stringify({ clubId })
    }),
  listSksClubApplications: (status?: string) => {
    const params = new URLSearchParams();
    if (status) params.set("status", status);
    const qs = params.toString();
    return request<SksClubApplication[]>(`/api/sks/club-applications${qs ? `?${qs}` : ""}`);
  },
  reviewSksClubApplication: (id: number, status: string, reviewNote?: string) =>
    request<{ message: string }>(`/api/sks/club-applications/${id}`, {
      method: "PATCH",
      body: JSON.stringify({ status, reviewNote })
    }),
  uploadEventDocument: (eventId: number, file: File) => {
    const formData = new FormData();
    formData.append("file", file);
    return uploadForm<{ message: string }>(`/api/events/${eventId}/documents`, formData);
  },
  listEventDocuments: (eventId: number) =>
    request<SksEventDocument[]>(`/api/events/${eventId}/documents`),
  listSksEventDocuments: (status?: string) => {
    const params = new URLSearchParams();
    if (status) params.set("status", status);
    const qs = params.toString();
    return request<SksEventDocument[]>(`/api/sks/event-documents${qs ? `?${qs}` : ""}`);
  },
  reviewSksEventDocument: (id: number, status: string, reviewNote?: string) =>
    request<{ message: string }>(`/api/sks/event-documents/${id}`, {
      method: "PATCH",
      body: JSON.stringify({ status, reviewNote })
    }),
  downloadEventDocument: async (id: number) => {
    const token = getToken();
    const res = await fetch(`${API_URL}/api/event-documents/${id}/download`, {
      headers: {
        ...(token ? { Authorization: `Bearer ${token}` } : {})
      }
    });

    if (!res.ok) {
      const text = await res.text();
      throw new Error(text || res.statusText);
    }
    return res.blob();
  },
  getAdminStats: () => request<AdminStats>(`/api/admin/stats`)
};
