"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ollamaGenerateTool = void 0;
const zod_1 = require("zod");
const callOllamaGenerate_js_1 = require("../helpers/callOllamaGenerate.js");
const textResponse_js_1 = require("../helpers/textResponse.js");
const schema = zod_1.z.object({
    model: zod_1.z.string().min(1),
    prompt: zod_1.z.string().min(1),
});
function isOllamaGenerateResponse(value) {
    if (typeof value !== "object" || value === null)
        return false;
    if (!("response" in value))
        return false;
    return (typeof value.response === "string" || typeof value.response === "undefined");
}
exports.ollamaGenerateTool = {
    name: "generate",
    description: "Generate text using Ollama",
    inputSchema: {
        type: "object",
        properties: {
            model: { type: "string" },
            prompt: { type: "string" },
        },
        required: ["model", "prompt"],
    },
    run: async (args) => {
        const parsed = schema.parse(args ?? {});
        const data = await (0, callOllamaGenerate_js_1.callOllamaGenerate)(parsed);
        if (isOllamaGenerateResponse(data))
            return (0, textResponse_js_1.textResponse)(data.response ?? "No response from Ollama");
        return (0, textResponse_js_1.textResponse)(data);
    },
};
