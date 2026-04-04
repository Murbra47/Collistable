<script setup lang="ts">
import { GoogleLogin } from "vue3-google-login";
import { useRouter } from "vue-router";
import { authService } from "../services/AuthService";
import { useAuthStore } from "../stores/auth";
import { useToastStore } from "../stores/toast";
import { styles } from "../library/CommonStyles";
import { strings } from "../library/CommonStrings";

const router = useRouter();
const authStore = useAuthStore();
const toast = useToastStore();

async function handleSuccess(response: { credential: string }) {
  try {
    const result = await authService.loginWithGoogle(response.credential);
    authStore.setAuth(result.token, {
      name: result.name,
      pictureUrl: result.pictureUrl,
    });
    router.push("/");
  } catch {
    toast.showToast(strings.AUTH.SIGN_IN_FAILED, "danger");
  }
}

function handleError() {
  toast.showToast(strings.AUTH.SIGN_IN_FAILED, "danger");
}

function loginWithGithub() {
  const state = crypto.randomUUID();
  sessionStorage.setItem("github_oauth_state", state);

  const clientId = import.meta.env.VITE_GITHUB_CLIENT_ID;
  const redirectUri = `${window.location.origin}/auth/github/callback`;
  const params = new URLSearchParams({ client_id: clientId, redirect_uri: redirectUri, scope: "user:email", state });
  window.location.href = `https://github.com/login/oauth/authorize?${params}`;
}
</script>

<template>
  <div :class="styles.COMPONENTS.LOGIN_CARD">
    <h1 :class="styles.SPACING.MB_2">{{ strings.NAV.APP_TITLE }}</h1>
    <p :class="[styles.TYPOGRAPHY.TEXT_MUTED, styles.SPACING.MB_3]">
      {{ strings.AUTH.SIGN_IN_TITLE }}
    </p>
    <GoogleLogin :callback="handleSuccess" @error="handleError" :width="400" />
    <button :class="[styles.BUTTON.BTN_DARK, styles.LAYOUT.W_100, styles.SPACING.MT_2, styles.COMPONENTS.GITHUB_BTN]" @click="loginWithGithub">
      <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true" style="fill:currentColor">
        <path fill-rule="evenodd" clip-rule="evenodd" d="M12 0C5.37 0 0 5.37 0 12c0 5.31 3.435 9.795 8.205 11.385.6.105.825-.255.825-.57 0-.285-.015-1.23-.015-2.235-3.015.555-3.795-.735-4.035-1.41-.135-.345-.72-1.41-1.23-1.695-.42-.225-1.02-.78-.015-.795.945-.015 1.62.87 1.845 1.23 1.08 1.815 2.805 1.305 3.495.99.105-.78.42-1.305.765-1.605-2.67-.3-5.46-1.335-5.46-5.925 0-1.305.465-2.385 1.23-3.225-.12-.3-.54-1.53.12-3.18 0 0 1.005-.315 3.3 1.23.96-.27 1.98-.405 3-.405s2.04.135 3 .405c2.295-1.56 3.3-1.23 3.3-1.23.66 1.65.24 2.88.12 3.18.765.84 1.23 1.905 1.23 3.225 0 4.605-2.805 5.625-5.475 5.925.435.375.81 1.095.81 2.22 0 1.605-.015 2.895-.015 3.3 0 .315.225.69.825.57A12.02 12.02 0 0 0 24 12c0-6.63-5.37-12-12-12z" />
      </svg>
      {{ strings.AUTH.GITHUB_SIGN_IN }}
    </button>
  </div>
</template>
