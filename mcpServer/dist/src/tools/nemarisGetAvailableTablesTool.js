"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.nemarisGetAvailableTablesTool = void 0;
const zod_1 = require("zod");
const callNemarisApi_js_1 = require("../helpers/callNemarisApi.js");
const loginAndGetToken_js_1 = require("../helpers/loginAndGetToken.js");
const textResponse_js_1 = require("../helpers/textResponse.js");
const isoDateTimeSchema = zod_1.z
    .string()
    .min(1)
    .refine((value) => !Number.isNaN(Date.parse(value)), {
    message: "Must be a valid ISO datetime string.",
});
const schema = zod_1.z.object({
    email: zod_1.z.string().min(1),
    password: zod_1.z.string().min(1),
    apiBaseUrl: zod_1.z.string().url().optional(),
    startTime: isoDateTimeSchema,
    endTime: isoDateTimeSchema,
    partySize: zod_1.z.number().int().positive(),
});
exports.nemarisGetAvailableTablesTool = {
    name: "nemaris_get_available_tables",
    description: "Login and fetch available restaurant tables for the requested time window.",
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
        const loginResult = await (0, loginAndGetToken_js_1.loginAndGetToken)(parsed);
        const query = new URLSearchParams({
            startTime: parsed.startTime,
            endTime: parsed.endTime,
            partySize: parsed.partySize.toString(),
        });
        const data = await (0, callNemarisApi_js_1.callNemarisApi)({
            apiBaseUrl: loginResult.apiBaseUrl,
            accessToken: loginResult.accessToken,
            path: `/api/reservations/available-tables?${query.toString()}`,
            method: "GET",
        });
        return (0, textResponse_js_1.textResponse)(data);
    },
};
