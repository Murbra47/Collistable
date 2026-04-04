import { ServiceBase } from "./ServiceBase";

interface AuthResponse {
  token: string;
  name: string;
  pictureUrl?: string | null;
}

export const authService = {
  loginWithGoogle: (credential: string) =>
    ServiceBase.post<AuthResponse>("/auth/google", { credential }),

  loginWithGithub: (code: string) =>
    ServiceBase.post<AuthResponse>("/auth/github", { code }),
};
