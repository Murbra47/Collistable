<script setup lang="ts">
import { ref, computed } from "vue";
import { RouterLink } from "vue-router";
import { useQuery, useMutation, useQueryClient } from "@tanstack/vue-query";
import type { Game } from "../types/Game";
import { gamesService } from "../services/GamesService";
import { useToastStore } from "../stores/toast";
import { styles } from "../library/CommonStyles";
import { strings } from "../library/CommonStrings";

type OwnedFilter = "all" | "owned" | "not-owned";
type WishlistFilter = "all" | "wish" | "not-wish";

const queryClient = useQueryClient();
const toast = useToastStore();

const platformFilter = ref<string>("All");
const ownedFilter = ref<OwnedFilter>("all");
const wishlistFilter = ref<WishlistFilter>("all");
const sortOption = ref<string>("title-asc");

const { data } = useQuery<Game[]>({
  queryKey: ["games"],
  queryFn: gamesService.getAll,
});

const platforms = computed(() => [
  ...new Set((data.value ?? []).map((g) => g.platform)),
]);

const filteredGames = computed(() =>
  (data.value ?? [])
    .filter((g) => platformFilter.value === "All" || g.platform === platformFilter.value)
    .filter((g) =>
      ownedFilter.value === "owned" ? g.owned :
      ownedFilter.value === "not-owned" ? !g.owned :
      true
    )
    .filter((g) =>
      wishlistFilter.value === "wish" ? g.wishlist :
      wishlistFilter.value === "not-wish" ? !g.wishlist :
      true
    )
    .sort((a, b) => {
      if (sortOption.value === "title-asc") return a.title.localeCompare(b.title);
      if (sortOption.value === "title-desc") return b.title.localeCompare(a.title);
      if (sortOption.value === "year-desc") return (b.releaseYear ?? 0) - (a.releaseYear ?? 0);
      if (sortOption.value === "year-asc") return (a.releaseYear ?? 0) - (b.releaseYear ?? 0);
      return 0;
    })
);

const { mutate: deleteGameMutate } = useMutation({
  mutationFn: gamesService.delete,
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ["games"] });
    toast.showToast(strings.GAMES.TOAST_DELETED, "success");
  },
  onError: () => {
    toast.showToast(strings.GAMES.TOAST_DELETE_FAILED, "danger");
  },
});

const { mutate: updateGameMutate } = useMutation({
  mutationFn: gamesService.update,
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ["games"] });
  },
});

function toggleOwned(game: Game) {
  updateGameMutate({ ...game, owned: !game.owned });
}

function toggleWishlist(game: Game) {
  updateGameMutate({ ...game, wishlist: !game.wishlist });
}

function confirmDelete(game: Game) {
  if (game.id !== undefined && confirm(strings.GAMES.DELETE_CONFIRM(game.title))) {
    deleteGameMutate(game.id);
  }
}
</script>

<template>
  <div v-if="!data?.length" :class="[styles.LAYOUT.CONTAINER, styles.SPACING.MT_5, styles.TYPOGRAPHY.TEXT_CENTER]">
    <h3>{{ strings.GAMES.EMPTY_COLLECTION }}</h3>
    <p :class="styles.TYPOGRAPHY.TEXT_MUTED">{{ strings.GAMES.EMPTY_COLLECTION_SUB }}</p>
    <RouterLink :class="[styles.BUTTON.BTN_PRIMARY, styles.BUTTON.BTN_LG, styles.SPACING.MT_3]" to="/search">
      {{ strings.GAMES.SEARCH_GAMES_EMOJI }}
    </RouterLink>
  </div>

  <div v-else :class="[styles.LAYOUT.CONTAINER, styles.SPACING.MT_4]">
    <div :class="[styles.LAYOUT.D_FLEX, styles.LAYOUT.JUSTIFY_CONTENT_BETWEEN, styles.LAYOUT.ALIGN_ITEMS_START, styles.SPACING.MB_3]">
      <div>
        <h2>{{ strings.GAMES.YOUR_COLLECTION }}</h2>
        <div :class="[styles.LAYOUT.D_FLEX, styles.LAYOUT.GAP_2, styles.SPACING.MT_2]">
          <select :class="[styles.FORM.FORM_SELECT_SM, styles.COMPONENTS.FILTER_SELECT]" v-model="platformFilter" aria-label="Filter by platform">
            <option value="All">{{ strings.FILTERS.ALL_PLATFORMS }}</option>
            <option v-for="p in platforms" :key="p" :value="p">{{ p }}</option>
          </select>

          <select :class="[styles.FORM.FORM_SELECT_SM, styles.COMPONENTS.FILTER_SELECT]" v-model="ownedFilter" aria-label="Filter by owned status">
            <option value="all">{{ strings.FILTERS.ALL }}</option>
            <option value="owned">{{ strings.FILTERS.OWNED }}</option>
            <option value="not-owned">{{ strings.FILTERS.NOT_OWNED }}</option>
          </select>

          <select :class="[styles.FORM.FORM_SELECT_SM, styles.COMPONENTS.FILTER_SELECT]" v-model="wishlistFilter" aria-label="Filter by wishlist status">
            <option value="all">{{ strings.FILTERS.ALL }}</option>
            <option value="wish">{{ strings.FILTERS.WISHLIST }}</option>
            <option value="not-wish">{{ strings.FILTERS.NOT_IN_WISHLIST }}</option>
          </select>

          <select :class="[styles.FORM.FORM_SELECT_SM, styles.COMPONENTS.SORT_SELECT]" v-model="sortOption" aria-label="Sort games">
            <option value="title-asc">{{ strings.FILTERS.TITLE_ASC }}</option>
            <option value="title-desc">{{ strings.FILTERS.TITLE_DESC }}</option>
            <option value="year-desc">{{ strings.FILTERS.NEWEST_FIRST }}</option>
            <option value="year-asc">{{ strings.FILTERS.OLDEST_FIRST }}</option>
          </select>
        </div>
      </div>
      <RouterLink :class="styles.BUTTON.BTN_PRIMARY" to="/search">{{ strings.GAMES.SEARCH_GAMES }}</RouterLink>
    </div>

    <div v-if="filteredGames.length === 0" :class="[styles.TYPOGRAPHY.TEXT_CENTER, styles.SPACING.MT_5]">
      <h4>{{ strings.GAMES.NO_FILTER_RESULTS }}</h4>
      <p :class="styles.TYPOGRAPHY.TEXT_MUTED">{{ strings.GAMES.NO_FILTER_RESULTS_SUB }}</p>
    </div>

    <div v-else :class="[styles.LAYOUT.ROW, styles.LAYOUT.G_3]">
      <div v-for="game in filteredGames" :key="game.id" :class="[styles.LAYOUT.COL_12, styles.LAYOUT.COL_SM_6, styles.LAYOUT.COL_MD_4, styles.LAYOUT.COL_LG_3]">
        <div :class="[styles.CARD.CARD, styles.LAYOUT.H_100, styles.POSITIONING.POSITION_RELATIVE, styles.BORDER.SHADOW_SM, styles.COMPONENTS.GAME_CARD]">
          <span v-if="game.owned" :class="[styles.BADGE.BADGE_SUCCESS, styles.POSITIONING.POSITION_ABSOLUTE, styles.POSITIONING.TOP_0, styles.POSITIONING.START_0, styles.SPACING.M_2]">
            {{ strings.GAMES.BADGE_OWNED }}
          </span>
          <span v-if="game.wishlist" :class="[styles.BADGE.BADGE_WARNING, styles.POSITIONING.POSITION_ABSOLUTE, styles.POSITIONING.TOP_0, styles.POSITIONING.END_0, styles.SPACING.M_2]">
            {{ strings.GAMES.BADGE_WISHLIST }}
          </span>

          <img
            :src="game.coverImageUrl ?? 'https://via.placeholder.com/150'"
            :class="styles.CARD.CARD_IMG_COVER"
            :alt="game.title"
          />

          <div :class="styles.CARD.CARD_BODY">
            <h5 :class="styles.CARD.CARD_TITLE">
              <RouterLink :to="`/games/${game.id}`" :class="[styles.TYPOGRAPHY.FW_BOLD, styles.TYPOGRAPHY.TEXT_DECORATION_NONE]">
                {{ game.title }}
              </RouterLink>
            </h5>
            <span :class="[styles.BADGE.BADGE_SECONDARY, styles.SPACING.MB_2]">{{ game.platform }}</span>

            <div :class="[styles.LAYOUT.D_FLEX, styles.LAYOUT.JUSTIFY_CONTENT_BETWEEN, styles.SPACING.MT_3]">
              <div :class="[styles.LAYOUT.D_FLEX, styles.LAYOUT.GAP_1]">
                <button
                  :class="game.owned ? styles.BUTTON.BTN_SM_SUCCESS : styles.BUTTON.BTN_SM_OUTLINE_SUCCESS"
                  :aria-label="game.owned ? `Mark ${game.title} as not owned` : `Mark ${game.title} as owned`"
                  :aria-pressed="game.owned"
                  @click="toggleOwned(game)"
                >✔</button>
                <button
                  :class="game.wishlist ? styles.BUTTON.BTN_SM_WARNING : styles.BUTTON.BTN_SM_OUTLINE_WARNING"
                  :aria-label="game.wishlist ? `Remove ${game.title} from wishlist` : `Add ${game.title} to wishlist`"
                  :aria-pressed="game.wishlist"
                  @click="toggleWishlist(game)"
                >⭐</button>
              </div>
              <div :class="[styles.LAYOUT.D_FLEX, styles.LAYOUT.GAP_1]">
                <RouterLink :class="styles.BUTTON.BTN_SM_OUTLINE_PRIMARY" :to="`/edit/${game.id}`">
                  {{ strings.COMMON.EDIT }}
                </RouterLink>
                <button :class="styles.BUTTON.BTN_SM_OUTLINE_DANGER" @click="confirmDelete(game)">
                  {{ strings.COMMON.DELETE }}
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

