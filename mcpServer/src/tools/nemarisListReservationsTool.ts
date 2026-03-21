import { z } from "zod";
import { callNemarisApi } from "../helpers/callNemarisApi.js";
import { loginAndGetToken } from "../helpers/loginAndGetToken.js";
import { textResponse } from "../helpers/textResponse.js";
import type { ToolDefinition } from "./toolDefinition.js";

const listDateSchema = z
  .string()
  .regex(/^\d{4}-\d{2}-\d{2}$/, "Must be in YYYY-MM-DD format.");

const schema = z.object({
  email: z.string().min(1),
  password: z.string().min(1),
  apiBaseUrl: z.string().url().optional(),
  fromDate: listDateSchema.optional(),
  toDate: listDateSchema.optional(),
});

export const nemarisListReservationsTool: ToolDefinition = {
  name: "nemaris_list_reservations",
  description:
    "Login and list reservations. Optional filters: fromDate/toDate (YYYY-MM-DD).",
  inputSchema: {
    type: "object",
    properties: {
      email: { type: "string" },
      password: { type: "string" },
      apiBaseUrl: { type: "string" },
      fromDate: { type: "string" },
      toDate: { type: "string" },
    },
    required: ["email", "password"],
  },
  run: async (args) => {
    const parsed = schema.parse(args ?? {});
    const loginResult = await loginAndGetToken(parsed);

    const query = new URLSearchParams();
    if (parsed.fromDate) query.set("fromDate", parsed.fromDate);
    if (parsed.toDate) query.set("toDate", parsed.toDate);

    const path = query.size
      ? `/api/reservations?${query.toString()}`
      : "/api/reservations";

    const data = await callNemarisApi({
      apiBaseUrl: loginResult.apiBaseUrl,
      accessToken: loginResult.accessToken,
      path,
      method: "GET",
    });

    return textResponse(data);
  },
};
