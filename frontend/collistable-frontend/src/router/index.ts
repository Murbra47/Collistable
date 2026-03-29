import { createRouter, createWebHistory } from "vue-router";
import GameList from "../pages/GameList.vue";
import SearchGames from "../pages/SearchGames.vue";
import GameCreateEdit from "../pages/GameCreateEdit.vue";
import GameDetails from "../pages/GameDetails.vue";
import Login from "../pages/Login.vue";
import { useAuthStore } from "../stores/auth";

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: "/", component: GameList },
    { path: "/search", component: SearchGames },
    { path: "/create", component: GameCreateEdit },
    { path: "/edit/:id", component: GameCreateEdit },
    { path: "/games/:id", component: GameDetails },
    { path: "/login", component: Login, meta: { public: true } },
  ],
});

router.beforeEach((to) => {
  const authStore = useAuthStore();
  if (!to.meta.public && !authStore.isAuthenticated) {
    return "/login";
  }
});
