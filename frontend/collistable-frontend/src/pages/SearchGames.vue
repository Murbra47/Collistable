<script setup lang="ts">
import { ref } from "vue";
import { useMutation, useQueryClient } from "@tanstack/vue-query";
import type { IGDBGame } from "../types/IGDB";
import { igdbService } from "../services/IgdbService";
import { gamesService } from "../services/GamesService";
import { useToastStore } from "../stores/toast";
import { styles } from "../library/CommonStyles";
import { strings } from "../library/CommonStrings";

const query = ref("");
const results = ref<IGDBGame[]>([]);
const loading = ref(false);

const queryClient = useQueryClient();
const toast = useToastStore();

const { mutate: doImport, isPending } = useMutation({
  mutationFn: gamesService.import,
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ["games"] });
    toast.showToast(strings.GAMES.TOAST_ADDED, "success");
  },
  onError: () => {
    toast.showToast(strings.GAMES.TOAST_ADD_FAILED, "danger");
  },
});

async function handleSearch() {
  loading.value = true;
  results.value = await igdbService.search(query.value);
  loading.value = false;
}

function toYear(unixSeconds?: number | null): number | null {
  if (!unixSeconds) return null;
  return new Date(unixSeconds * 1000).getUTCFullYear();
}

function coverUrl(game: IGDBGame): string {
  return game.cover
    ? `https://images.igdb.com/igdb/image/upload/t_cover_big/${game.cover.image_id}.jpg`
    : "https://via.placeholder.com/150";
}
</script>

<template>
  <div :class="[styles.LAYOUT.CONTAINER, styles.SPACING.MT_4]">
    <h2>{{ strings.GAMES.SEARCH_GAMES }}</h2>

    <form @submit.prevent="handleSearch" :class="[styles.FORM.INPUT_GROUP, styles.SPACING.MB_3]">
      <label for="game-search" :class="styles.TYPOGRAPHY.VISUALLY_HIDDEN">{{ strings.GAMES.SEARCH_GAMES }}</label>
      <input
        id="game-search"
        :class="styles.FORM.FORM_CONTROL"
        :placeholder="strings.GAMES.SEARCH_PLACEHOLDER"
        v-model="query"
      />
      <button :class="styles.BUTTON.BTN_PRIMARY" type="submit" :disabled="loading">
        {{ loading ? strings.GAMES.SEARCHING : strings.NAV.SEARCH_GAMES }}
      </button>
    </form>

    <div aria-live="polite" aria-atomic="true" :class="styles.TYPOGRAPHY.VISUALLY_HIDDEN">
      <span v-if="!loading && results.length > 0">{{ results.length }} results found</span>
      <span v-else-if="!loading && query && results.length === 0">No results found</span>
    </div>

    <div :class="[styles.LAYOUT.ROW, styles.LAYOUT.G_3]">
      <div v-for="game in results" :key="game.id" :class="[styles.LAYOUT.COL_12, styles.LAYOUT.COL_SM_6, styles.LAYOUT.COL_MD_4, styles.LAYOUT.COL_LG_3]">
        <div :class="[styles.CARD.CARD, styles.LAYOUT.H_100, styles.BORDER.SHADOW_SM]">
          <img
            :src="coverUrl(game)"
            :class="styles.CARD.CARD_IMG_COVER"
            :alt="game.name"
          />
          <div :class="styles.CARD.CARD_BODY">
            <h5 :class="styles.CARD.CARD_TITLE">{{ game.name }}</h5>
            <p :class="[styles.CARD.CARD_TEXT, styles.TYPOGRAPHY.TEXT_MUTED]">
              {{ game.platforms?.map((p) => p.name).join(", ") ?? strings.GAMES.UNKNOWN_PLATFORM }}
            </p>
            <button
              :class="[styles.BUTTON.BTN_SUCCESS, styles.LAYOUT.W_100]"
              :disabled="isPending"
              @click="doImport({
                title: game.name,
                platform: game.platforms?.[0]?.name ?? strings.GAMES.UNKNOWN_PLATFORM,
                igdbId: game.id,
                coverImageUrl: coverUrl(game),
                summary: game.summary ?? null,
                releaseYear: toYear(game.first_release_date),
                owned: false,
                wishlist: false,
              })"
            >
              {{ isPending ? strings.GAMES.ADDING : strings.GAMES.ADD_TO_COLLECTION }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
