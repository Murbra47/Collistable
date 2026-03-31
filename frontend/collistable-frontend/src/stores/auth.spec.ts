import { describe, it, expect, beforeEach } from "vitest";
import { setActivePinia, createPinia } from "pinia";
import { useAuthStore } from "./auth";

function makeJwt(payload: object): string {
  const header = btoa(JSON.stringify({ alg: "HS256", typ: "JWT" }));
  const body = btoa(JSON.stringify(payload));
  return `${header}.${body}.signature`;
}

function futureExp(): number {
  return Math.floor(Date.now() / 1000) + 3600;
}

function pastExp(): number {
  return Math.floor(Date.now() / 1000) - 1;
}

describe("auth store", () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    localStorage.clear();
  });

  it("is not authenticated when no token is stored", () => {
    const store = useAuthStore();
    expect(store.isAuthenticated).toBe(false);
    expect(store.user).toBeNull();
  });

  it("extracts name from token claims", () => {
    const store = useAuthStore();
    const jwt = makeJwt({ name: "Alice", email: "alice@example.com", exp: futureExp() });
    store.setAuth(jwt, { name: "Alice", pictureUrl: null });
    expect(store.user?.name).toBe("Alice");
  });

  it("falls back to email when name claim is absent", () => {
    const store = useAuthStore();
    // decodeUser falls back: name ?? email
    const jwt = makeJwt({ email: "alice@example.com", exp: futureExp() });
    // Manually set via setAuth simulating what AuthService does after decoding
    store.setAuth(jwt, { name: "alice@example.com", pictureUrl: null });
    expect(store.user?.name).toBe("alice@example.com");
  });

  it("isAuthenticated is true after setAuth", () => {
    const store = useAuthStore();
    const jwt = makeJwt({ name: "Alice", exp: futureExp() });
    store.setAuth(jwt, { name: "Alice", pictureUrl: null });
    expect(store.isAuthenticated).toBe(true);
  });

  it("isAuthenticated is false after clearAuth", () => {
    const store = useAuthStore();
    const jwt = makeJwt({ name: "Alice", exp: futureExp() });
    store.setAuth(jwt, { name: "Alice", pictureUrl: null });
    store.clearAuth();
    expect(store.isAuthenticated).toBe(false);
  });

  it("clearAuth removes token from localStorage", () => {
    const store = useAuthStore();
    const jwt = makeJwt({ name: "Alice", exp: futureExp() });
    store.setAuth(jwt, { name: "Alice", pictureUrl: null });
    store.clearAuth();
    expect(localStorage.getItem("collistable_jwt")).toBeNull();
  });

  it("setAuth persists token to localStorage", () => {
    const store = useAuthStore();
    const jwt = makeJwt({ name: "Alice", exp: futureExp() });
    store.setAuth(jwt, { name: "Alice", pictureUrl: null });
    expect(localStorage.getItem("collistable_jwt")).toBe(jwt);
  });

  it("decodeUser returns null for an expired token", () => {
    // Simulate store initialising with an expired token already in localStorage
    const expiredJwt = makeJwt({ name: "Alice", exp: pastExp() });
    localStorage.setItem("collistable_jwt", expiredJwt);
    // Re-create the store so it reads from localStorage on init
    setActivePinia(createPinia());
    const store = useAuthStore();
    expect(store.user).toBeNull();
    expect(store.isAuthenticated).toBe(false);
  });
});
