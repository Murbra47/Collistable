import { defineStore } from "pinia";
import { ref, computed } from "vue";
import type { AuthUser } from "../types/AuthUser";

const TOKEN_KEY = "collistable_jwt";

function decodeUser(jwt: string): AuthUser | null {
  try {
    const payload = JSON.parse(atob(jwt.split(".")[1]));
    if (payload.exp && Date.now() / 1000 > payload.exp) return null;
    return {
      name: payload.name ?? payload.email,
      pictureUrl: payload.picture ?? null,
    };
  } catch {
    return null;
  }
}

export const useAuthStore = defineStore("auth", () => {
  const storedToken = localStorage.getItem(TOKEN_KEY);

  const token = ref<string | null>(storedToken);
  const user = ref<AuthUser | null>(storedToken ? decodeUser(storedToken) : null);

  const isAuthenticated = computed(() => token.value !== null && user.value !== null);

  function setAuth(newToken: string, newUser: AuthUser) {
    token.value = newToken;
    user.value = newUser;
    localStorage.setItem(TOKEN_KEY, newToken);
  }

  function clearAuth() {
    token.value = null;
    user.value = null;
    localStorage.removeItem(TOKEN_KEY);
  }

  return { token, user, isAuthenticated, setAuth, clearAuth };
});
