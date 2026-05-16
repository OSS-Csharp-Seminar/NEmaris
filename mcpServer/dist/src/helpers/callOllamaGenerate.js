"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.callOllamaGenerate = callOllamaGenerate;
const parseResponseBody_js_1 = require("./parseResponseBody.js");
async function callOllamaGenerate(request) {
    const response = await fetch("http://localhost:11434/api/generate", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify({
            model: request.model,
            prompt: request.prompt,
            stream: false,
        }),
    });
    const parsed = await (0, parseResponseBody_js_1.parseResponseBody)(response);
    if (!response.ok) {
        throw new Error(`Ollama error ${response.status}: ${typeof parsed === "string" ? parsed : JSON.stringify(parsed)}`);
    }
    return parsed;
}
