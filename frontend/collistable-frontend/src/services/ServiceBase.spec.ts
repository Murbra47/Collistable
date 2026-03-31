import { describe, it, expect, beforeEach, vi } from "vitest";
import { setActivePinia, createPinia } from "pinia";
import { useAuthStore } from "../stores/auth";

// Mock the router to avoid full Vue app setup
vi.mock("../router", () => ({
  router: { push: vi.fn() },
}));

// Import ServiceBase after mocks are in place
const { ServiceBase } = await import("./ServiceBase");

function makeJwt(payload: object): string {
  const header = btoa(JSON.stringify({ alg: "HS256", typ: "JWT" }));
  const body = btoa(JSON.stringify(payload));
  return `${header}.${body}.signature`;
}

describe("ServiceBase", () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    localStorage.clear();
    vi.clearAllMocks();
  });

  it("throws and clears auth on 401", async () => {
    const store = useAuthStore();
    const jwt = makeJwt({ name: "Alice", exp: Math.floor(Date.now() / 1000) + 3600 });
    store.setAuth(jwt, { name: "Alice", pictureUrl: null });

    globalThis.fetch = vi.fn().mockResolvedValue({ status: 401, ok: false });

    await expect(ServiceBase.get("/games")).rejects.toThrow("Unauthorized");
    expect(store.isAuthenticated).toBe(false);
  });

  it("redirects to /login on 401", async () => {
    const store = useAuthStore();
    store.setAuth(makeJwt({ name: "Alice", exp: Math.floor(Date.now() / 1000) + 3600 }), {
      name: "Alice",
      pictureUrl: null,
    });

    globalThis.fetch = vi.fn().mockResolvedValue({ status: 401, ok: false });

    await expect(ServiceBase.get("/games")).rejects.toThrow("Unauthorized");

    const { router } = await import("../router");
    expect(router.push).toHaveBeenCalledWith("/login");
  });

  it("throws on non-ok response", async () => {
    globalThis.fetch = vi.fn().mockResolvedValue({ status: 500, ok: false });
    await expect(ServiceBase.get("/games")).rejects.toThrow("Request failed: 500");
  });

  it("returns undefined for 204 No Content", async () => {
    globalThis.fetch = vi.fn().mockResolvedValue({ status: 204, ok: true });
    const result = await ServiceBase.delete("/games/1");
    expect(result).toBeUndefined();
  });

  it("returns parsed JSON on success", async () => {
    const data = [{ id: 1, title: "Halo" }];
    globalThis.fetch = vi.fn().mockResolvedValue({
      status: 200,
      ok: true,
      json: () => Promise.resolve(data),
    });
    const result = await ServiceBase.get("/games");
    expect(result).toEqual(data);
  });

  it("attaches Authorization header when token is set", async () => {
    const store = useAuthStore();
    const jwt = makeJwt({ name: "Alice", exp: Math.floor(Date.now() / 1000) + 3600 });
    store.setAuth(jwt, { name: "Alice", pictureUrl: null });

    const data = {};
    globalThis.fetch = vi.fn().mockResolvedValue({
      status: 200,
      ok: true,
      json: () => Promise.resolve(data),
    });

    await ServiceBase.get("/games");

    const [, options] = (globalThis.fetch as ReturnType<typeof vi.fn>).mock.calls[0];
    expect(options.headers["Authorization"]).toBe(`Bearer ${jwt}`);
  });
});
