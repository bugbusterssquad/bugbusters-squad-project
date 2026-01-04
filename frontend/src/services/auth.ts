import type { User } from "../types";

export type AuthPayload = {
  token: string;
  user: User;
};

const AUTH_KEY = "clubs_auth";

export function getAuth(): AuthPayload | null {
  const raw = localStorage.getItem(AUTH_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as AuthPayload;
  } catch {
    return null;
  }
}

export function setAuth(payload: AuthPayload) {
  localStorage.setItem(AUTH_KEY, JSON.stringify(payload));
}

export function updateStoredUser(update: Partial<User>) {
  const current = getAuth();
  if (!current) return;
  const next = { ...current, user: { ...current.user, ...update } };
  localStorage.setItem(AUTH_KEY, JSON.stringify(next));
}

export function clearAuth() {
  localStorage.removeItem(AUTH_KEY);
}

export function getUser(): User | null {
  return getAuth()?.user ?? null;
}

export function getToken(): string | null {
  return getAuth()?.token ?? null;
}
