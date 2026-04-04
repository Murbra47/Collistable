<script setup lang="ts">
import { onMounted } from "vue";
import { useRouter, useRoute } from "vue-router";
import { authService } from "../services/AuthService";
import { useAuthStore } from "../stores/auth";
import { useToastStore } from "../stores/toast";
import { styles } from "../library/CommonStyles";
import { strings } from "../library/CommonStrings";

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();
const toast = useToastStore();

onMounted(async () => {
  const code = route.query.code as string | undefined;
  const state = route.query.state as string | undefined;
  const expectedState = sessionStorage.getItem("github_oauth_state");
  sessionStorage.removeItem("github_oauth_state");

  if (!code || !state || state !== expectedState) {
    toast.showToast(strings.AUTH.SIGN_IN_FAILED, "danger");
    router.push("/login");
    return;
  }

  try {
    const result = await authService.loginWithGithub(code);
    authStore.setAuth(result.token, {
      name: result.name,
      pictureUrl: result.pictureUrl,
    });
    router.push("/");
  } catch {
    toast.showToast(strings.AUTH.SIGN_IN_FAILED, "danger");
    router.push("/login");
  }
});
</script>

<template>
  <div :class="styles.COMPONENTS.LOGIN_CARD">
    <p :class="styles.TYPOGRAPHY.TEXT_MUTED">{{ strings.AUTH.SIGNING_IN }}</p>
  </div>
</template>
