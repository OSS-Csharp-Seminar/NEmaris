import { getApiBaseUrl } from "./getApiBaseUrl.js";
import { parseResponseBody } from "./parseResponseBody.js";

export type LoginCredentials = {
  email: string;
  password: string;
  apiBaseUrl?: string;
};

export type LoginResult = {
  apiBaseUrl: string;
  accessToken: string;
};

type LoginApiResponse = {
  accessToken: string;
};

function isLoginApiResponse(value: unknown): value is LoginApiResponse {
  if (typeof value !== "object" || value === null) return false;
  return "accessToken" in value && typeof value.accessToken === "string";
}

export async function loginAndGetToken(
  credentials: LoginCredentials
): Promise<LoginResult> {
  const apiBaseUrl = getApiBaseUrl(credentials.apiBaseUrl);

  const response = await fetch(`${apiBaseUrl}/api/auth/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      email: credentials.email,
      password: credentials.password,
    }),
  });

  const parsed = await parseResponseBody(response);
  if (!response.ok) {
    throw new Error(
      `Login failed (${response.status}): ${
        typeof parsed === "string" ? parsed : JSON.stringify(parsed)
      }`
    );
  }

  if (!isLoginApiResponse(parsed)) {
    throw new Error("Login response does not contain accessToken.");
  }

  return {
    apiBaseUrl,
    accessToken: parsed.accessToken,
  };
}
