import axios from "axios";
import { tokenStorage } from "./tokenStorage";

const API_BASE_URL = import.meta.env.VITE_API_URL ?? "/api";

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: { "Content-Type": "application/json" },
});

// ---------- REQUEST INTERCEPTOR ----------
// Attaches the access token to every outgoing request
api.interceptors.request.use(
  (config) => {
    const token = tokenStorage.getAccessToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// ---------- RESPONSE INTERCEPTOR ----------
// On a 401, tries to refresh the token once, then retries the original request.
// If refresh also fails, clears tokens and redirects to /login.
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (token: string) => void;
  reject: (err: unknown) => void;
}> = [];

const processQueue = (error: unknown, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (token) prom.resolve(token);
    else prom.reject(error);
  });
  failedQueue = [];
};

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // Only attempt refresh on 401 and not on the refresh endpoint itself
    if (
      error.response?.status !== 401 ||
      originalRequest._retry ||
      originalRequest.url?.includes("/auth/refresh") ||
      originalRequest.url?.includes("/auth/login")
    ) {
      return Promise.reject(error);
    }

    // If a refresh is already in flight, queue this request
    if (isRefreshing) {
      return new Promise<string>((resolve, reject) => {
        failedQueue.push({ resolve, reject });
      }).then((token) => {
        originalRequest.headers.Authorization = `Bearer ${token}`;
        return api(originalRequest);
      });
    }

    originalRequest._retry = true;
    isRefreshing = true;

    const refreshTokenAtStart = tokenStorage.getRefreshToken();
    try {
      if (!refreshTokenAtStart) {
        const err = new Error("No refresh token");
        (err as Error & { isAuthFailure?: boolean }).isAuthFailure = true;
        throw err;
      }

      const { data } = await axios.post<{
        accessToken: string;
        refreshToken: string;
      }>(`${API_BASE_URL}/auth/refresh`, { refreshToken: refreshTokenAtStart });

      tokenStorage.setTokens(data.accessToken, data.refreshToken);
      processQueue(null, data.accessToken);

      originalRequest.headers.Authorization = `Bearer ${data.accessToken}`;
      return api(originalRequest);
    } catch (refreshError) {
      // Another tab may have rotated the refresh token while we were trying.
      // If so, retry the original request with the new access token instead of logging out.
      const refreshTokenNow = tokenStorage.getRefreshToken();
      const accessTokenNow = tokenStorage.getAccessToken();
      if (
        refreshTokenNow &&
        refreshTokenNow !== refreshTokenAtStart &&
        accessTokenNow
      ) {
        processQueue(null, accessTokenNow);
        originalRequest.headers.Authorization = `Bearer ${accessTokenNow}`;
        return api(originalRequest);
      }

      processQueue(refreshError, null);

      const refreshStatus = (refreshError as { response?: { status?: number } })
        .response?.status;
      const isExplicitAuthFailure =
        refreshStatus === 401 ||
        (refreshError as { isAuthFailure?: boolean }).isAuthFailure === true;

      if (isExplicitAuthFailure) {
        tokenStorage.clearTokens();
        window.location.href = "/login";
      }

      return Promise.reject(refreshError);
    } finally {
      isRefreshing = false;
    }
  }
);

export default api;
