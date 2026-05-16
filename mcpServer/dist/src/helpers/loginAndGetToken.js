"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.loginAndGetToken = loginAndGetToken;
const getApiBaseUrl_js_1 = require("./getApiBaseUrl.js");
const parseResponseBody_js_1 = require("./parseResponseBody.js");
function isLoginApiResponse(value) {
    if (typeof value !== "object" || value === null)
        return false;
    return "accessToken" in value && typeof value.accessToken === "string";
}
async function loginAndGetToken(credentials) {
    const apiBaseUrl = (0, getApiBaseUrl_js_1.getApiBaseUrl)(credentials.apiBaseUrl);
    const response = await fetch(`${apiBaseUrl}/api/auth/login`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify({
            email: credentials.email,
            password: credentials.password,
        }),
    });
    const parsed = await (0, parseResponseBody_js_1.parseResponseBody)(response);
    if (!response.ok) {
        throw new Error(`Login failed (${response.status}): ${typeof parsed === "string" ? parsed : JSON.stringify(parsed)}`);
    }
    if (!isLoginApiResponse(parsed)) {
        throw new Error("Login response does not contain accessToken.");
    }
    return {
        apiBaseUrl,
        accessToken: parsed.accessToken,
    };
}
