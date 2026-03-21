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
  firstName: z.string().min(1),
  lastName: z.string().min(1),
  phone: z.string().min(1),
  guestEmail: z.string().email().optional(),
  notes: z.string().optional(),
  tableId: z.number().int().positive(),
  startTime: isoDateTimeSchema,
  endTime: isoDateTimeSchema,
  partySize: z.number().int().positive(),
  specialRequest: z.string().optional(),
});

export const nemarisCreateReservationTool: ToolDefinition = {
  name: "nemaris_create_reservation",
  description:
    "Login and create a reservation. Guest is created/updated by phone number.",
  inputSchema: {
    type: "object",
    properties: {
      email: { type: "string" },
      password: { type: "string" },
      apiBaseUrl: { type: "string" },
      firstName: { type: "string" },
      lastName: { type: "string" },
      phone: { type: "string" },
      guestEmail: { type: "string" },
      notes: { type: "string" },
      tableId: { type: "number" },
      startTime: {
        type: "string",
        description: "ISO datetime, e.g. 2026-03-25T18:00:00Z",
      },
      endTime: {
        type: "string",
        description: "ISO datetime, e.g. 2026-03-25T20:00:00Z",
      },
      partySize: { type: "number" },
      specialRequest: { type: "string" },
    },
    required: [
      "email",
      "password",
      "firstName",
      "lastName",
      "phone",
      "tableId",
      "startTime",
      "endTime",
      "partySize",
    ],
  },
  run: async (args) => {
    const parsed = schema.parse(args ?? {});
    const loginResult = await loginAndGetToken(parsed);

    const data = await callNemarisApi({
      apiBaseUrl: loginResult.apiBaseUrl,
      accessToken: loginResult.accessToken,
      path: "/api/reservations",
      method: "POST",
      body: {
        firstName: parsed.firstName,
        lastName: parsed.lastName,
        phone: parsed.phone,
        email: parsed.guestEmail,
        notes: parsed.notes,
        tableId: parsed.tableId,
        startTime: parsed.startTime,
        endTime: parsed.endTime,
        partySize: parsed.partySize,
        specialRequest: parsed.specialRequest,
      },
    });

    return textResponse(data);
  },
};
