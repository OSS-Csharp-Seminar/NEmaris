import { z } from "zod";
import { callNemarisApi } from "../helpers/callNemarisApi.js";
import { loginAndGetToken } from "../helpers/loginAndGetToken.js";
import { textResponse } from "../helpers/textResponse.js";
import type { ToolDefinition } from "./toolDefinition.js";

const isoDateTimeSchema = z
  .string()
  .min(1)
  .refine((value) => !Number.isNaN(Date.parse(value)), {
    message: "Must be a valid ISO datetime string.",
  });

const schema = z.object({
  email: z.string().min(1),
  password: z.string().min(1),
  apiBaseUrl: z.string().url().optional(),
  startTime: isoDateTimeSchema,
  endTime: isoDateTimeSchema,
  partySize: z.number().int().positive(),
});

export const nemarisGetAvailableTablesTool: ToolDefinition = {
  name: "nemaris_get_available_tables",
  description:
    "Login and fetch available restaurant tables for the requested time window.",
  inputSchema: {
    type: "object",
    properties: {
      email: { type: "string" },
      password: { type: "string" },
      apiBaseUrl: { type: "string" },
      startTime: {
        type: "string",
        description: "ISO datetime, e.g. 2026-03-25T18:00:00Z",
      },
      endTime: {
        type: "string",
        description: "ISO datetime, e.g. 2026-03-25T20:00:00Z",
      },
      partySize: { type: "number" },
    },
    required: ["email", "password", "startTime", "endTime", "partySize"],
  },
  run: async (args) => {
    const parsed = schema.parse(args ?? {});
    const loginResult = await loginAndGetToken(parsed);

    const query = new URLSearchParams({
      startTime: parsed.startTime,
      endTime: parsed.endTime,
      partySize: parsed.partySize.toString(),
    });

    const data = await callNemarisApi({
      apiBaseUrl: loginResult.apiBaseUrl,
      accessToken: loginResult.accessToken,
      path: `/api/reservations/available-tables?${query.toString()}`,
      method: "GET",
    });

    return textResponse(data);
  },
};
