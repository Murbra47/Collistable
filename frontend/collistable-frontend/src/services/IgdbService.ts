import type { IGDBGame } from "../types/IGDB";
import { ServiceBase } from "./ServiceBase";

export const igdbService = {
  search: (query: string) =>
    ServiceBase.get<IGDBGame[]>(`/igdb/search?q=${encodeURIComponent(query)}`),
};
