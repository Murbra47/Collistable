<script setup lang="ts">
import { useRoute, RouterLink } from "vue-router";
import { useQuery, useMutation, useQueryClient } from "@tanstack/vue-query";
import type { Game } from "../types/Game";
import { gamesService } from "../services/GamesService";
import { styles } from "../library/CommonStyles";
import { strings } from "../library/CommonStrings";

const route = useRoute();
const id = route.params.id as string;
const queryClient = useQueryClient();

const { data: game, isLoading } = useQuery<Game>({
  queryKey: ["game", id],
  queryFn: () => gamesService.getById(id),
});

const { mutate } = useMutation({
  mutationFn: gamesService.update,
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ["game", id] });
  },
});

function toggleOwned() {
  if (!game.value) return;
  mutate({ ...game.value, owned: !game.value.owned });
}

function toggleWishlist() {
  if (!game.value) return;
  mutate({ ...game.value, wishlist: !game.value.wishlist });
}
</script>

<template>
  <p v-if="isLoading">{{ strings.COMMON.LOADING }}</p>
  <p v-else-if="!game">{{ strings.GAMES.GAME_NOT_FOUND }}</p>

  <div v-else :class="[styles.LAYOUT.CONTAINER, styles.SPACING.MT_4]">
    <div :class="[styles.LAYOUT.D_FLEX, styles.LAYOUT.GAP_4]">
      <img
        :src="game.coverImageUrl ?? 'https://via.placeholder.com/250'"
        :class="[styles.BORDER.ROUNDED, styles.BORDER.SHADOW, styles.COMPONENTS.COVER_IMAGE]"
        :alt="game.title"
      />

      <div>
        <h2>{{ game.title }}</h2>
        <p :class="[styles.TYPOGRAPHY.TEXT_MUTED, styles.SPACING.MB_3]">
          {{ game.platform }}{{ game.releaseYear ? ` • ${game.releaseYear}` : "" }}
        </p>

        <p v-if="game.summary" :class="styles.COMPONENTS.GAME_SUMMARY">{{ game.summary }}</p>

        <div :class="[styles.SPACING.MT_3, styles.LAYOUT.D_FLEX, styles.LAYOUT.GAP_2]">
          <button
            :class="game.owned ? styles.BUTTON.BTN_SUCCESS : styles.BUTTON.BTN_OUTLINE_SUCCESS"
            :aria-pressed="game.owned"
            @click="toggleOwned"
          >
            {{ game.owned ? strings.GAMES.OWNED : strings.GAMES.MARK_OWNED }}
          </button>

          <button
            :class="game.wishlist ? styles.BUTTON.BTN_WARNING : styles.BUTTON.BTN_OUTLINE_WARNING"
            :aria-pressed="game.wishlist"
            @click="toggleWishlist"
          >
            {{ game.wishlist ? strings.GAMES.WISHLISTED : strings.GAMES.ADD_TO_WISHLIST }}
          </button>

          <RouterLink :to="`/edit/${game.id}`" :class="styles.BUTTON.BTN_PRIMARY">
            {{ strings.GAMES.EDIT_GAME }}
          </RouterLink>
        </div>

        <div :class="styles.SPACING.MT_3">
          <RouterLink to="/">{{ strings.GAMES.BACK_TO_GAMES }}</RouterLink>
        </div>
      </div>
    </div>
  </div>
</template>

