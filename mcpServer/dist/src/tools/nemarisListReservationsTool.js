"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.nemarisListReservationsTool = void 0;
const zod_1 = require("zod");
const callNemarisApi_js_1 = require("../helpers/callNemarisApi.js");
const loginAndGetToken_js_1 = require("../helpers/loginAndGetToken.js");
const textResponse_js_1 = require("../helpers/textResponse.js");
const listDateSchema = zod_1.z
    .string()
    .regex(/^\d{4}-\d{2}-\d{2}$/, "Must be in YYYY-MM-DD format.");
const schema = zod_1.z.object({
    email: zod_1.z.string().min(1),
    password: zod_1.z.string().min(1),
    apiBaseUrl: zod_1.z.string().url().optional(),
    fromDate: listDateSchema.optional(),
    toDate: listDateSchema.optional(),
});
exports.nemarisListReservationsTool = {
    name: "nemaris_list_reservations",
    description: "Login and list reservations. Optional filters: fromDate/toDate (YYYY-MM-DD).",
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
        const loginResult = await (0, loginAndGetToken_js_1.loginAndGetToken)(parsed);
        const query = new URLSearchParams();
        if (parsed.fromDate)
            query.set("fromDate", parsed.fromDate);
        if (parsed.toDate)
            query.set("toDate", parsed.toDate);
        const path = query.size
            ? `/api/reservations?${query.toString()}`
            : "/api/reservations";
        const data = await (0, callNemarisApi_js_1.callNemarisApi)({
            apiBaseUrl: loginResult.apiBaseUrl,
            accessToken: loginResult.accessToken,
            path,
            method: "GET",
        });
        return (0, textResponse_js_1.textResponse)(data);
    },
};
