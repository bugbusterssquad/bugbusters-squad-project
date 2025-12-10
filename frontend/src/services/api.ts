const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5084";

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const res = await fetch(`${API_URL}${path}`, {
    headers: {
      "Content-Type": "application/json",
      ...(options.headers || {})
    },
    credentials: "include",
    ...options
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || res.statusText);
  }
  return (await res.json()) as T;
}

export const api = {
  listClubs: () => request<Club[]>("/api/clubs"),
  getClub: (id: number) => request<Club>(`/api/clubs/${id}`),
  register: (name: string, email: string, password: string) =>
    request<{ message: string }>("/api/auth/register", {
      method: "POST",
      body: JSON.stringify({ Name: name, Email: email, Password: password })
    }),
  login: (email: string, password: string) =>
    request<{ id: number; name: string; email: string }>("/api/auth/login", {
      method: "POST",
      body: JSON.stringify({ Email: email, Password: password })
    }),
  applyMembership: (userId: number, clubId: number) =>
    request<{ message: string }>("/api/membership/apply", {
      method: "POST",
      body: JSON.stringify({ UserId: userId, ClubId: clubId })
    }),
  getUserMembership: (userId: number) =>
    request<{ id: number; clubId: number; status: string; qrCodeBase64?: string | null }>(
      `/api/membership/user/${userId}`
    )
};
