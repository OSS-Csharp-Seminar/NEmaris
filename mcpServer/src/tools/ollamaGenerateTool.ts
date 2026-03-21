import { z } from "zod";
import { callOllamaGenerate } from "../helpers/callOllamaGenerate.js";
import { textResponse } from "../helpers/textResponse.js";
import type { ToolDefinition } from "./toolDefinition.js";

const schema = z.object({
  model: z.string().min(1),
  prompt: z.string().min(1),
});

type OllamaGenerateResponse = {
  response?: string;
};

function isOllamaGenerateResponse(
  value: unknown
): value is OllamaGenerateResponse {
  if (typeof value !== "object" || value === null) return false;
  if (!("response" in value)) return false;
  return (
    typeof value.response === "string" || typeof value.response === "undefined"
  );
}

export const ollamaGenerateTool: ToolDefinition = {
  name: "generate",
  description: "Generate text using Ollama",
  inputSchema: {
    type: "object",
    properties: {
      model: { type: "string" },
      prompt: { type: "string" },
    },
    required: ["model", "prompt"],
  },
  run: async (args) => {
    const parsed = schema.parse(args ?? {});
    const data = await callOllamaGenerate(parsed);

    if (isOllamaGenerateResponse(data))
      return textResponse(data.response ?? "No response from Ollama");

    return textResponse(data);
  },
};
