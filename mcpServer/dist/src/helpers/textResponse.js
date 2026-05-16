"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.textResponse = textResponse;
function textResponse(payload) {
    return {
        content: [
            {
                type: "text",
                text: typeof payload === "string" ? payload : JSON.stringify(payload, null, 2),
            },
        ],
    };
}
