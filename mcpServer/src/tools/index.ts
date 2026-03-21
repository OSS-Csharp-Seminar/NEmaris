import type { ToolDefinition } from "./toolDefinition.js";
import { nemarisCreateReservationTool } from "./nemarisCreateReservationTool.js";
import { nemarisGetAvailableTablesTool } from "./nemarisGetAvailableTablesTool.js";
import { nemarisListReservationsTool } from "./nemarisListReservationsTool.js";
import { nemarisLoginTool } from "./nemarisLoginTool.js";
import { ollamaGenerateTool } from "./ollamaGenerateTool.js";

export const allTools: ToolDefinition[] = [
  nemarisLoginTool,
  nemarisGetAvailableTablesTool,
  nemarisCreateReservationTool,
  nemarisListReservationsTool,
  ollamaGenerateTool,
];
