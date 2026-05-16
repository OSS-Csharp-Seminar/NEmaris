"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.parseResponseBody = parseResponseBody;
async function parseResponseBody(response) {
    const text = await response.text();
    if (!text)
        return {};
    try {
        return JSON.parse(text);
    }
    catch {
        return text;
    }
}
