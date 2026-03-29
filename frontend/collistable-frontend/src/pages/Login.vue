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
</script>

<template>
  <div :class="styles.COMPONENTS.LOGIN_CARD">
    <h1 :class="styles.SPACING.MB_2">{{ strings.NAV.APP_TITLE }}</h1>
    <p :class="[styles.TYPOGRAPHY.TEXT_MUTED, styles.SPACING.MB_3]">
      {{ strings.AUTH.SIGN_IN_TITLE }}
    </p>
    <GoogleLogin :callback="handleSuccess" @error="handleError" />
  </div>
</template>
