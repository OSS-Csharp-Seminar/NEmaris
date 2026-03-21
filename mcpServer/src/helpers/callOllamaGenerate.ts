import { parseResponseBody } from "./parseResponseBody.js";

export type OllamaGenerateRequest = {
  model: string;
  prompt: string;
};

export async function callOllamaGenerate(
  request: OllamaGenerateRequest
): Promise<unknown> {
  const response = await fetch("http://localhost:11434/api/generate", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      model: request.model,
      prompt: request.prompt,
      stream: false,
    }),
  });

  const parsed = await parseResponseBody(response);
  if (!response.ok) {
    throw new Error(
      `Ollama error ${response.status}: ${
        typeof parsed === "string" ? parsed : JSON.stringify(parsed)
      }`
    );
  }

  return parsed;
}
