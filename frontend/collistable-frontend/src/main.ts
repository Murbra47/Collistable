import { createApp } from "vue";
import { createPinia } from "pinia";
import { VueQueryPlugin } from "@tanstack/vue-query";
import vue3GoogleLogin from "vue3-google-login";
import { router } from "./router";
import "bootstrap-icons/font/bootstrap-icons.css";
import "./assets/styles/main.scss";
import App from "./App.vue";

const app = createApp(App);
app.use(createPinia());
app.use(router);
app.use(VueQueryPlugin);
app.use(vue3GoogleLogin, { clientId: import.meta.env.VITE_GOOGLE_CLIENT_ID });
app.mount("#app");
