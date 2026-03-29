<script setup lang="ts">
import { RouterView, RouterLink, useRouter } from "vue-router";
import { useToastStore } from "./stores/toast";
import { useAuthStore } from "./stores/auth";
import { styles } from "./library/CommonStyles";
import { strings } from "./library/CommonStrings";

const toast = useToastStore();
const authStore = useAuthStore();
const router = useRouter();

function signOut() {
  authStore.clearAuth();
  router.push("/login");
}
</script>

<template>
  <div :class="styles.COMPONENTS.APP_WRAPPER">
    <nav v-if="authStore.isAuthenticated" :class="styles.COMPONENTS.APP_NAV" aria-label="Main navigation">
      <RouterLink to="/" :class="styles.COMPONENTS.APP_BRAND">
        <i class="bi bi-collection-fill"></i>
        {{ strings.NAV.APP_TITLE }}
      </RouterLink>

      <div :class="styles.COMPONENTS.NAV_LINKS">
        <RouterLink to="/">{{ strings.NAV.GAMES }}</RouterLink>
        <RouterLink to="/search">{{ strings.NAV.SEARCH_GAMES }}</RouterLink>
      </div>

      <span :class="styles.COMPONENTS.NAV_USER">
        <img
          v-if="authStore.user?.pictureUrl"
          :src="authStore.user.pictureUrl"
          :class="styles.COMPONENTS.NAV_AVATAR"
          alt="User avatar"
          @error="(e) => ((e.target as HTMLImageElement).style.display = 'none')"
        />
        {{ authStore.user?.name }}
        <button :class="styles.BUTTON.BTN_OUTLINE_LIGHT" @click="signOut">
          {{ strings.NAV.SIGN_OUT }}
        </button>
      </span>
    </nav>
    <main class="page-content">
      <RouterView />
    </main>
  </div>

  <div
    :class="[
      styles.POSITIONING.POSITION_FIXED,
      styles.POSITIONING.BOTTOM_0,
      styles.POSITIONING.END_0,
      styles.SPACING.P_3,
      styles.COMPONENTS.TOAST_CONTAINER,
    ]"
  >
    <div
      v-for="t in toast.toasts"
      :key="t.id"
      :class="[
        styles.TOAST.TOAST_SHOW,
        styles.LAYOUT.ALIGN_ITEMS_CENTER,
        styles.TYPOGRAPHY.TEXT_WHITE,
        `bg-${t.type}`,
        styles.BORDER.BORDER_0,
        styles.SPACING.MB_2,
      ]"
      role="alert"
    >
      <div :class="styles.LAYOUT.D_FLEX">
        <div :class="styles.TOAST.TOAST_BODY">{{ t.message }}</div>
        <button
          type="button"
          :class="[
            styles.BUTTON.BTN_CLOSE_WHITE,
            styles.SPACING.ME_2,
            styles.SPACING.M_AUTO,
          ]"
          aria-label="Dismiss notification"
          @click="toast.dismiss(t.id)"
        >
          <span aria-hidden="true">&times;</span>
        </button>
      </div>
    </div>
  </div>
</template>
