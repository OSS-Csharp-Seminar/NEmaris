"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.callNemarisApi = callNemarisApi;
const parseResponseBody_js_1 = require("./parseResponseBody.js");
async function callNemarisApi(request) {
    const response = await fetch(`${request.apiBaseUrl}${request.path}`, {
        method: request.method,
        headers: {
            Authorization: `Bearer ${request.accessToken}`,
            "Content-Type": "application/json",
        },
        body: request.body ? JSON.stringify(request.body) : undefined,
    });
    const parsed = await (0, parseResponseBody_js_1.parseResponseBody)(response);
    if (!response.ok) {
        throw new Error(`NEmaris API failed (${response.status}) on ${request.path}: ${typeof parsed === "string" ? parsed : JSON.stringify(parsed)}`);
    }
    return parsed;
}
