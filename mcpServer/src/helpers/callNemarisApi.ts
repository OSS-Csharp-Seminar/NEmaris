import { parseResponseBody } from "./parseResponseBody.js";

export type NemarisApiRequest = {
  apiBaseUrl: string;
  accessToken: string;
  path: string;
  method: "GET" | "POST";
  body?: unknown;
};

export async function callNemarisApi(request: NemarisApiRequest): Promise<unknown> {
  const response = await fetch(`${request.apiBaseUrl}${request.path}`, {
    method: request.method,
    headers: {
      Authorization: `Bearer ${request.accessToken}`,
      "Content-Type": "application/json",
    },
    body: request.body ? JSON.stringify(request.body) : undefined,
  });

  const parsed = await parseResponseBody(response);
  if (!response.ok) {
    throw new Error(
      `NEmaris API failed (${response.status}) on ${request.path}: ${
        typeof parsed === "string" ? parsed : JSON.stringify(parsed)
      }`
    );
  }

  return parsed;
}
