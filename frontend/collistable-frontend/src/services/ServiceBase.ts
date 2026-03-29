import { useAuthStore } from "../stores/auth";

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5053/api";

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const authStore = useAuthStore();

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
  };

  if (authStore.token) {
    headers["Authorization"] = `Bearer ${authStore.token}`;
  }

  const res = await fetch(`${BASE_URL}${path}`, {
    ...options,
    headers: { ...((options?.headers as Record<string, string>) ?? {}), ...headers },
  });

  if (res.status === 401) {
    authStore.clearAuth();
    // Lazy import to avoid circular dependency between ServiceBase and router
    const { router } = await import("../router");
    router.push("/login");
    throw new Error("Unauthorized");
  }

  if (!res.ok) throw new Error(`Request failed: ${res.status}`);
  if (res.status === 204) return undefined as T;
  return res.json() as Promise<T>;
}

export const ServiceBase = {
  get: <T>(path: string) =>
    request<T>(path),
  post: <T>(path: string, body: unknown) =>
    request<T>(path, { method: "POST", body: JSON.stringify(body) }),
  put: <T>(path: string, body: unknown) =>
    request<T>(path, { method: "PUT", body: JSON.stringify(body) }),
  delete: (path: string) =>
    request<void>(path, { method: "DELETE" }),
};
