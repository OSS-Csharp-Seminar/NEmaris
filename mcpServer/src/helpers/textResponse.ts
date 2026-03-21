import type { ToolResponse } from "../tools/toolDefinition.js";

export function textResponse(payload: unknown): ToolResponse {
  return {
    content: [
      {
        type: "text",
        text:
          typeof payload === "string" ? payload : JSON.stringify(payload, null, 2),
      },
    ],
  };
}
