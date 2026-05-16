import { useState, useCallback, type ReactNode } from "react";
import { tokenStorage } from "../services/tokenStorage";
import authService, {
  type LoginRequest,
  type RegisterRequest,
} from "../services/authService";
import {
  AuthContext,
  type AuthContextType,
  type User,
} from "./auth-context";

function parseJwt(token: string): Record<string, string> {
  try {
    const base64Url = token.split(".")[1];
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const json = decodeURIComponent(
      atob(base64)
        .split("")
        .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
        .join("")
    );
    return JSON.parse(json);
  } catch {
    return {};
  }
}

function userFromToken(accessToken: string): User | null {
  const claims = parseJwt(accessToken);
  if (!claims.sub) return null;

  const role =
    claims[
      "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    ] ??
    claims.role ??
    "Guest";

  return {
    id: claims.sub,
    email: claims.email ?? "",
    firstName: claims.firstName ?? "",
    lastName: claims.lastName ?? "",
    role,
  };
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(() => {
    const token = tokenStorage.getAccessToken();
    return token ? userFromToken(token) : null;
  });

  const login = useCallback(async (data: LoginRequest) => {
    const res = await authService.login(data);
    tokenStorage.setTokens(res.data.accessToken, res.data.refreshToken);
    setUser(userFromToken(res.data.accessToken));
  }, []);

  const register = useCallback(async (data: RegisterRequest) => {
    const res = await authService.register(data);
    return res.data.message;
  }, []);

  const logout = useCallback(async () => {
    const refreshToken = tokenStorage.getRefreshToken();

    if (refreshToken) {
      try {
        await authService.revoke(refreshToken);
      } catch {
        // Local logout should still complete if the server-side revoke fails.
      }
    }

    tokenStorage.clearTokens();
    setUser(null);
  }, []);

  const value: AuthContextType = {
    user,
    isAuthenticated: !!user,
    isAdmin: user?.role === "Admin",
    isLoading: false,
    login,
    register,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
