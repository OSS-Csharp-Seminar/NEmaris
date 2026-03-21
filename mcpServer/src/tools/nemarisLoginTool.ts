import { z } from "zod";
import { loginAndGetToken } from "../helpers/loginAndGetToken.js";
import { textResponse } from "../helpers/textResponse.js";
import type { ToolDefinition } from "./toolDefinition.js";

const schema = z.object({
  email: z.string().min(1),
  password: z.string().min(1),
  apiBaseUrl: z.string().url().optional(),
});

export const nemarisLoginTool: ToolDefinition = {
  name: "nemaris_login",
  description: "Login chatbot user and return JWT token from NEmaris API.",
  inputSchema: {
    type: "object",
    properties: {
      email: { type: "string" },
      password: { type: "string" },
      apiBaseUrl: {
        type: "string",
        description: "Optional API base URL. Default: http://localhost:5199",
      },
    },
    required: ["email", "password"],
  },
  run: async (args) => {
    const parsed = schema.parse(args ?? {});
    const loginResult = await loginAndGetToken(parsed);
    return textResponse(loginResult);
  },
};
