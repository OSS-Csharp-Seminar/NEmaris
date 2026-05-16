"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.getApiBaseUrl = getApiBaseUrl;
const DEFAULT_NEMARIS_API_BASE_URL = "http://localhost:5199";
function getApiBaseUrl(override) {
    return override ?? process.env.NEMARIS_API_BASE_URL ?? DEFAULT_NEMARIS_API_BASE_URL;
}
