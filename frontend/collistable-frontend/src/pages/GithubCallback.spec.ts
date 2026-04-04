import { describe, it, expect, vi, beforeEach } from "vitest";
import { mount, flushPromises } from "@vue/test-utils";
import { setActivePinia, createPinia } from "pinia";
import { createRouter, createMemoryHistory } from "vue-router";
import GithubCallback from "./GithubCallback.vue";
import { authService } from "../services/AuthService";
import { useAuthStore } from "../stores/auth";
import { useToastStore } from "../stores/toast";
import { strings } from "../library/CommonStrings";

vi.mock("../services/AuthService", () => ({
  authService: {
    loginWithGithub: vi.fn(),
  },
}));

function makeRouter(initialPath: string) {
  const router = createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: "/", component: { template: "<div/>" } },
      { path: "/login", component: { template: "<div/>" } },
      { path: "/auth/github/callback", component: GithubCallback },
    ],
  });
  router.push(initialPath);
  return router;
}

async function mountCallback(query: Record<string, string> = {}) {
  const qs = new URLSearchParams(query).toString();
  const router = makeRouter(`/auth/github/callback${qs ? "?" + qs : ""}`);
  await router.isReady();
  const wrapper = mount(GithubCallback, { global: { plugins: [router] } });
  await flushPromises();
  return { wrapper, router };
}

describe("GithubCallback", () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    sessionStorage.clear();
    localStorage.clear();
    vi.clearAllMocks();
  });

  it("calls loginWithGithub with the code and navigates to / on success", async () => {
    sessionStorage.setItem("github_oauth_state", "abc123");
    vi.mocked(authService.loginWithGithub).mockResolvedValue({
      token: "jwt",
      name: "Alice",
      pictureUrl: null,
    });

    const { router } = await mountCallback({ code: "gh-code", state: "abc123" });

    expect(authService.loginWithGithub).toHaveBeenCalledWith("gh-code");
    expect(router.currentRoute.value.path).toBe("/");
  });

  it("sets auth in the store on success", async () => {
    sessionStorage.setItem("github_oauth_state", "abc123");
    vi.mocked(authService.loginWithGithub).mockResolvedValue({
      token: "jwt",
      name: "Alice",
      pictureUrl: "https://example.com/avatar.png",
    });

    await mountCallback({ code: "gh-code", state: "abc123" });

    const authStore = useAuthStore();
    expect(authStore.isAuthenticated).toBe(true);
  });

  it("redirects to /login when code is missing", async () => {
    sessionStorage.setItem("github_oauth_state", "abc123");

    const { router } = await mountCallback({ state: "abc123" });

    expect(authService.loginWithGithub).not.toHaveBeenCalled();
    expect(router.currentRoute.value.path).toBe("/login");
  });

  it("redirects to /login when state does not match sessionStorage", async () => {
    sessionStorage.setItem("github_oauth_state", "expected-state");

    const { router } = await mountCallback({ code: "gh-code", state: "wrong-state" });

    expect(authService.loginWithGithub).not.toHaveBeenCalled();
    expect(router.currentRoute.value.path).toBe("/login");
  });

  it("redirects to /login when no state is stored in sessionStorage", async () => {
    const { router } = await mountCallback({ code: "gh-code", state: "abc123" });

    expect(authService.loginWithGithub).not.toHaveBeenCalled();
    expect(router.currentRoute.value.path).toBe("/login");
  });

  it("shows error toast and redirects to /login when loginWithGithub throws", async () => {
    sessionStorage.setItem("github_oauth_state", "abc123");
    vi.mocked(authService.loginWithGithub).mockRejectedValue(new Error("Network error"));

    const { router } = await mountCallback({ code: "gh-code", state: "abc123" });

    const toastStore = useToastStore();
    expect(toastStore.toasts[0]?.message).toBe(strings.AUTH.SIGN_IN_FAILED);
    expect(router.currentRoute.value.path).toBe("/login");
  });

  it("clears state from sessionStorage after the callback completes", async () => {
    sessionStorage.setItem("github_oauth_state", "abc123");
    vi.mocked(authService.loginWithGithub).mockResolvedValue({
      token: "jwt",
      name: "Alice",
      pictureUrl: null,
    });

    await mountCallback({ code: "gh-code", state: "abc123" });

    expect(sessionStorage.getItem("github_oauth_state")).toBeNull();
  });
});
