export const GAMES = {
  // Headings
  YOUR_COLLECTION: "Your Game Collection",
  SEARCH_GAMES: "Search Games",
  SEARCH_GAMES_EMOJI: "Search Games 🎮",
  ADD_GAME: "Add Game",
  EDIT_GAME: "Edit Game",

  // Empty / filter states
  EMPTY_COLLECTION: "Your collection is empty 😢",
  EMPTY_COLLECTION_SUB: "Start adding some games to your library!",
  NO_FILTER_RESULTS: "No games match your filters 🎮",
  NO_FILTER_RESULTS_SUB: "Try adjusting your filters.",
  LOADING_GAME: "Loading game...",
  GAME_NOT_FOUND: "Game not found",

  // Search
  SEARCH_PLACEHOLDER: "Search for a game...",
  SEARCHING: "Searching...",
  UNKNOWN_PLATFORM: "Unknown Platform",

  // Badges
  BADGE_OWNED: "✔ Owned",
  BADGE_WISHLIST: "⭐ Wishlist",

  // Buttons
  ADD_TO_COLLECTION: "Add to Collection",
  ADDING: "Adding...",
  MARK_OWNED: "Mark Owned",
  OWNED: "Owned ✔",
  ADD_TO_WISHLIST: "Add to Wishlist",
  WISHLISTED: "Wishlisted ⭐",
  BACK_TO_GAMES: "← Back to Games",

  // Confirm dialogs
  DELETE_CONFIRM: (title: string) => `Delete ${title}?`,

  // Form labels
  LABEL_TITLE: "Title",
  LABEL_PLATFORM: "Platform",

  // Toasts
  TOAST_ADDED: "Game added to collection!",
  TOAST_ADD_FAILED: "Failed to add game.",
  TOAST_DELETED: "Game deleted.",
  TOAST_DELETE_FAILED: "Failed to delete game.",
  TOAST_CREATED: "Game created!",
  TOAST_CREATE_FAILED: "Failed to create game.",
  TOAST_UPDATED: "Game updated!",
  TOAST_UPDATE_FAILED: "Failed to update game.",
} as const;
