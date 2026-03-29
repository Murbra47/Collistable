export interface Game {
  id?: number;

  // Core metadata
  title: string;
  platform: string;
  releaseYear?: number | null;

  // IGDB metadata
  igdbId?: number | null;
  coverImageUrl?: string | null;
  summary?: string | null;

  // Collection tracking
  owned?: boolean;
  wishlist?: boolean;

  // Auditing (optional client-side usage)
  createdAt?: string; // Date sent as ISO string from API
  updatedAt?: string;
}
