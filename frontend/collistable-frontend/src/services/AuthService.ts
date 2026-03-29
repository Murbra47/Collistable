import { ServiceBase } from "./ServiceBase";

interface GoogleLoginResponse {
  token: string;
  name: string;
  pictureUrl?: string | null;
}

export const authService = {
  loginWithGoogle: (credential: string) =>
    ServiceBase.post<GoogleLoginResponse>("/auth/google", { credential }),
};
