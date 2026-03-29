export interface IGDBPlatform {
  name: string;
}

export interface IGDBCover {
  image_id: string;
}

export interface IGDBGame {
  id: number;
  name: string;

  // New fields from updated IGDB query
  summary?: string | null;
  first_release_date?: number | null;

  platforms?: IGDBPlatform[];
  cover?: IGDBCover;
}
