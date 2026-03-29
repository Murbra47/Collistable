<script setup lang="ts">
import { ref, computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useMutation, useQuery, useQueryClient } from "@tanstack/vue-query";
import type { Game } from "../types/Game";
import { gamesService } from "../services/GamesService";
import { useToastStore } from "../stores/toast";
import { styles } from "../library/CommonStyles";
import { strings } from "../library/CommonStrings";

const route = useRoute();
const router = useRouter();
const queryClient = useQueryClient();
const toast = useToastStore();

const id = route.params.id as string | undefined;
const isEditMode = computed(() => Boolean(id));

const { data, isError: queryError } = useQuery<Game>({
  queryKey: ["game", id],
  queryFn: () => gamesService.getById(id!),
  enabled: isEditMode,
});

const titleOverride = ref<string | null>(null);
const platformOverride = ref<string | null>(null);
const submitted = ref(false);

const titleValue = computed({
  get: () => titleOverride.value ?? data.value?.title ?? "",
  set: (v) => { titleOverride.value = v; },
});

const platformValue = computed({
  get: () => platformOverride.value ?? data.value?.platform ?? "",
  set: (v) => { platformOverride.value = v; },
});

const { mutate, isPending } = useMutation<Game, unknown, Game>({
  mutationFn: (game) => isEditMode.value ? gamesService.update(game) : gamesService.create(game),
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ["games"] });
    if (isEditMode.value) queryClient.invalidateQueries({ queryKey: ["game", id] });
    toast.showToast(isEditMode.value ? strings.GAMES.TOAST_UPDATED : strings.GAMES.TOAST_CREATED, "success");
    router.push("/");
  },
  onError: () => {
    toast.showToast(isEditMode.value ? strings.GAMES.TOAST_UPDATE_FAILED : strings.GAMES.TOAST_CREATE_FAILED, "danger");
  },
});

function handleSubmit(event: Event) {
  submitted.value = true;
  if (!(event.target as HTMLFormElement).checkValidity()) return;
  if (isEditMode.value && data.value) {
    mutate({
      ...data.value,
      title: titleOverride.value ?? data.value.title,
      platform: platformOverride.value ?? data.value.platform,
    });
  } else {
    mutate({
      title: titleOverride.value ?? "",
      platform: platformOverride.value ?? strings.GAMES.UNKNOWN_PLATFORM,
      owned: false,
      wishlist: false,
    });
  }
}
</script>

<template>
  <p v-if="isEditMode && queryError" :class="[styles.TYPOGRAPHY.TEXT_CENTER, styles.SPACING.MT_5]">
    {{ strings.GAMES.GAME_NOT_FOUND }}
  </p>
  <p v-else-if="isEditMode && !data" :class="[styles.TYPOGRAPHY.TEXT_CENTER, styles.SPACING.MT_5]">
    {{ strings.GAMES.LOADING_GAME }}
  </p>

  <div v-else :class="[styles.LAYOUT.CONTAINER, styles.SPACING.MT_4]">
    <div :class="[styles.CARD.CARD, styles.BORDER.SHADOW_SM, styles.LAYOUT.MX_AUTO, styles.COMPONENTS.FORM_CARD]">
      <div :class="styles.CARD.CARD_BODY">
        <h2 :class="[styles.CARD.CARD_TITLE, styles.SPACING.MB_3]">
          {{ isEditMode ? strings.GAMES.EDIT_GAME : strings.GAMES.ADD_GAME }}
        </h2>

        <form @submit.prevent="handleSubmit" :class="{ [styles.FORM.WAS_VALIDATED]: submitted }" novalidate>
          <p :class="styles.FORM.REQUIRED_NOTE"><span aria-hidden="true">*</span> {{ strings.COMMON.REQUIRED }}</p>

          <div :class="styles.SPACING.MB_3">
            <label for="game-title" :class="styles.FORM.FORM_LABEL">{{ strings.GAMES.LABEL_TITLE }}</label>
            <input id="game-title" required aria-required="true" :class="styles.FORM.FORM_CONTROL" v-model="titleValue" />
          </div>

          <div :class="styles.SPACING.MB_3">
            <label for="game-platform" :class="styles.FORM.FORM_LABEL">{{ strings.GAMES.LABEL_PLATFORM }}</label>
            <input id="game-platform" required aria-required="true" :class="styles.FORM.FORM_CONTROL" v-model="platformValue" />
          </div>

          <div :class="[styles.LAYOUT.D_FLEX, styles.LAYOUT.JUSTIFY_CONTENT_END, styles.LAYOUT.GAP_2]">
            <button type="button" :class="styles.BUTTON.BTN_OUTLINE_SECONDARY" @click="router.push('/')">
              {{ strings.COMMON.CANCEL }}
            </button>
            <button type="submit" :class="styles.BUTTON.BTN_PRIMARY" :disabled="isPending">
              {{ isPending ? strings.COMMON.SAVING : strings.COMMON.SAVE }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

