import type { Game } from "../types/Game";
import { ServiceBase } from "./ServiceBase";

export const gamesService = {
  getAll: () =>
    ServiceBase.get<Game[]>("/games"),
  getById: (id: string) =>
    ServiceBase.get<Game>(`/games/${id}`),
  create: (game: Game) =>
    ServiceBase.post<Game>("/games", game),
  update: (game: Game) =>
    ServiceBase.put<Game>(`/games/${game.id}`, game),
  delete: (id: number) =>
    ServiceBase.delete(`/games/${id}`),
  import: (game: Game) =>
    ServiceBase.post<void>("/games/import", game),
};
