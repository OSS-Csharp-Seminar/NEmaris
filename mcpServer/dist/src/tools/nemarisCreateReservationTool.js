"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.nemarisCreateReservationTool = void 0;
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
    firstName: zod_1.z.string().min(1),
    lastName: zod_1.z.string().min(1),
    phone: zod_1.z.string().min(1),
    guestEmail: zod_1.z.string().email().optional(),
    notes: zod_1.z.string().optional(),
    tableId: zod_1.z.number().int().positive(),
    startTime: isoDateTimeSchema,
    endTime: isoDateTimeSchema,
    partySize: zod_1.z.number().int().positive(),
    specialRequest: zod_1.z.string().optional(),
});
exports.nemarisCreateReservationTool = {
    name: "nemaris_create_reservation",
    description: "Login and create a reservation. Guest is created/updated by phone number.",
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
        const loginResult = await (0, loginAndGetToken_js_1.loginAndGetToken)(parsed);
        const data = await (0, callNemarisApi_js_1.callNemarisApi)({
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
        return (0, textResponse_js_1.textResponse)(data);
    },
};
