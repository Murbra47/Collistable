import { describe, it, expect, vi, beforeEach } from "vitest";
import { setActivePinia, createPinia } from "pinia";

vi.mock("../router", () => ({
  router: { push: vi.fn() },
}));

const { authService } = await import("./AuthService");

describe("authService", () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    localStorage.clear();
    vi.clearAllMocks();
  });

  it("loginWithGoogle posts credential to /auth/google", async () => {
    const expected = { token: "jwt", name: "Alice", pictureUrl: null };
    globalThis.fetch = vi.fn().mockResolvedValue({
      status: 200,
      ok: true,
      json: () => Promise.resolve(expected),
    });

    const result = await authService.loginWithGoogle("google-credential");

    expect(result).toEqual(expected);
    const [url, options] = (globalThis.fetch as ReturnType<typeof vi.fn>).mock.calls[0];
    expect(url).toContain("/auth/google");
    expect(JSON.parse(options.body)).toEqual({ credential: "google-credential" });
  });

  it("loginWithGithub posts code to /auth/github", async () => {
    const expected = { token: "jwt", name: "Bob", pictureUrl: "https://example.com/avatar.png" };
    globalThis.fetch = vi.fn().mockResolvedValue({
      status: 200,
      ok: true,
      json: () => Promise.resolve(expected),
    });

    const result = await authService.loginWithGithub("github-code");

    expect(result).toEqual(expected);
    const [url, options] = (globalThis.fetch as ReturnType<typeof vi.fn>).mock.calls[0];
    expect(url).toContain("/auth/github");
    expect(JSON.parse(options.body)).toEqual({ code: "github-code" });
  });

  it("loginWithGoogle throws on non-ok response", async () => {
    globalThis.fetch = vi.fn().mockResolvedValue({ status: 401, ok: false });
    await expect(authService.loginWithGoogle("bad-credential")).rejects.toThrow();
  });

  it("loginWithGithub throws on non-ok response", async () => {
    globalThis.fetch = vi.fn().mockResolvedValue({ status: 401, ok: false });
    await expect(authService.loginWithGithub("bad-code")).rejects.toThrow();
  });
});
