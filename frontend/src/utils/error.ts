export function getErrorMessage(error: unknown, fallback = "Hata oluştu") {
  if (error instanceof Error) return error.message || fallback;
  if (typeof error === "string") return error || fallback;
  return fallback;
}
